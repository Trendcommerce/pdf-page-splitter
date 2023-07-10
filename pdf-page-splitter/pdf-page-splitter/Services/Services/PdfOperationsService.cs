using iText.Kernel.Pdf.Canvas.Parser;
using iText.Kernel.Pdf.Canvas.Parser.Filter;
using iText.Kernel.Pdf.Canvas.Parser.Listener;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using pdf_page_splitter.Data.Domain;
using pdf_page_splitter.Extensions;
using pdf_page_splitter.Infrastructure;
using pdf_page_splitter.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace pdf_page_splitter.Services.Services
{
    public class PdfOperationsService : IPdfOperationsService
    {
        private readonly ILogger<PdfOperationsService> _logger;
        private readonly IUnitOfWork _unitOfWork;
        private readonly string _inputFilesPath;
        private readonly string _outputFilesPath;
        private readonly string _archivedFilesPath;
        private readonly string _rejectedFilesPath;

        public PdfOperationsService(ILogger<PdfOperationsService> logger, IConfiguration configuration, IUnitOfWork unitOfWork)
        {
            var appSettings = configuration.Get<AppSettings>() ?? new AppSettings();
            var foldersSection = appSettings.Folders;
            _inputFilesPath = foldersSection.InputFilesPath ?? throw new ArgumentNullException(nameof(foldersSection.InputFilesPath));
            _outputFilesPath = foldersSection.OutputFilesPath ?? throw new ArgumentNullException(nameof(foldersSection.OutputFilesPath));
            _archivedFilesPath = foldersSection.ArchivedFilesPath ?? throw new ArgumentNullException(nameof(foldersSection.ArchivedFilesPath));
            _rejectedFilesPath = foldersSection.RejectedFilesPath ?? throw new ArgumentNullException(nameof(foldersSection.RejectedFilesPath));

            FileHelper.CreateDirectory(_archivedFilesPath);
            FileHelper.CreateDirectory(_rejectedFilesPath);

            _logger = logger;
            _unitOfWork = unitOfWork;
        }

        public async Task Run()
        {
            try
            {
                _logger.LogInformation("pdf-page-splitter Run...");

                if (!Directory.Exists(_inputFilesPath))
                    throw new Exception("Input folder not exist!");

                var inputFolderDirectoryInfo = new DirectoryInfo(_inputFilesPath);
                var allPdfFilesInInputFolder = inputFolderDirectoryInfo.GetFiles("*.pdf", SearchOption.TopDirectoryOnly);
                if (allPdfFilesInInputFolder.Length > 0)
                {
                    var uploadedFiles = await _unitOfWork.UploadedFile.GetAllExecutedAsync(true).ConfigureAwait(false);
                    foreach (var file in allPdfFilesInInputFolder)
                    {
                        var fileName = file.Name;
                        _logger.LogInformation($"pdf-page-splitter working on '{fileName}'..");

                        #region upload file Exist in db

                        if (uploadedFiles.Any(x => x.Name == fileName))
                        {
                            var message = $"'{fileName}' is exist in db..";
                            _logger.LogError(new Exception(message), message);

                            var lastFileName = FileHelper.NextAvailableFilename(Path.Combine(_rejectedFilesPath, fileName));
                            file.MoveTo(lastFileName);
                            _logger.LogWarning($"'{fileName}' has been moved to the rejected folder with the name '{lastFileName}.");

                            continue;
                        }

                        #endregion

                        var uploadedFile = new UploadedFile();
                        uploadedFile.Name = fileName;
                        uploadedFile.Status = AppConstant.UploadedFileStatusExecuting;
                        await _unitOfWork.UploadedFile.CreateAsync(uploadedFile).ConfigureAwait(false);

                        try
                        {
                            var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(fileName);
                            var outputFolderPath = Path.Combine(_outputFilesPath, fileNameWithoutExtension);
                            var whiteLabelsByDocumentIdList = FindWhiteLabels(file.FullName);
                            await CreateResultPdf(file.FullName, outputFolderPath, whiteLabelsByDocumentIdList, uploadedFile.Id).ConfigureAwait(false);

                            #region Move pdf file to archived folder and update status

                            file.MoveTo(Path.Combine(_archivedFilesPath, fileName));
                            _logger.LogInformation($"'{fileName}' was moved to the archived folder.");

                            uploadedFile.Status = AppConstant.UploadedFileStatusExecuted;
                            await _unitOfWork.UploadedFile.UpdateAsync(uploadedFile).ConfigureAwait(false);

                            #endregion
                        }
                        catch (Exception e)
                        {
                            _logger.LogError(e, $"{fileName} - {e.Message}");

                            var lastFileName = FileHelper.NextAvailableFilename(Path.Combine(_rejectedFilesPath, fileName));
                            file.MoveTo(lastFileName);
                            _logger.LogWarning($"'{fileName}' has been moved to the rejected folder with the name '{lastFileName}.");

                            uploadedFile.FailedReason = e.Message;
                            uploadedFile.Status = AppConstant.UploadedFileStatusFailed;
                            await _unitOfWork.UploadedFile.UpdateAsync(uploadedFile).ConfigureAwait(false);
                        }
                    }
                }

                _logger.LogInformation("pdf-page-splitter Stop...");
            }
            catch (Exception e)
            {
                _logger.LogError(e, e.Message);
            }
        }

        #region Helper

        //(0, 0) coordinate as a lower-left corner and (595, 842) as an upper-right corner a4 page
        private static List<WhiteLabelsByDocumentId> FindWhiteLabels(string pdfFilePath)
        {
            string whiteLabelText = null;
            var pageNumber = 0;

            using var pdfFileStream = new FileStream(pdfFilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            var pdfDocument = new iText.Kernel.Pdf.PdfDocument(new iText.Kernel.Pdf.PdfReader(pdfFileStream));
            try
            {                                                    // (0,        7.5,    105,      7.5) for a4 actual page sizes
                var rectWhiteLabel = new iText.Kernel.Geom.Rectangle(0f, 820.7325f, 297.5f, 21.2675f); // for whitelabel area 
                string documentId = null;
                var whiteLabelPerPageList = new List<WhiteLabelPerPage>();
                var whiteLabelsByDocumentIdList = new List<WhiteLabelsByDocumentId>();
                for (pageNumber = 1; pageNumber <= pdfDocument.GetNumberOfPages(); pageNumber++)
                {
                    var filterWhiteLabel = new TextRegionEventFilter(rectWhiteLabel);
                    var listenerWhiteLabel = new FilteredEventListener();
                    var strategyWhiteLabel = listenerWhiteLabel.AttachEventListener(new LocationTextExtractionStrategy(), filterWhiteLabel);
                    new PdfCanvasProcessor(listenerWhiteLabel).ProcessPageContent(pdfDocument.GetPage(pageNumber));

                    whiteLabelText = Encoding.UTF8.GetString(Encoding.UTF8.GetBytes(strategyWhiteLabel.GetResultantText()));
                    var whiteLabelRecord = SplitTextFromString(whiteLabelText);
                    if (documentId != whiteLabelRecord.Id)
                    {
                        if (whiteLabelPerPageList.Count > 0)
                        {
                            whiteLabelsByDocumentIdList.Add(new WhiteLabelsByDocumentId(documentId, whiteLabelPerPageList));
                        }

                        whiteLabelPerPageList = new List<WhiteLabelPerPage>();
                        documentId = whiteLabelRecord.Id;

                        if (documentId.IsNullOrEmpty())
                            throw new ArgumentNullException(nameof(documentId));

                        whiteLabelPerPageList.Add(new WhiteLabelPerPage(whiteLabelRecord, pageNumber));
                    }
                    else
                    {
                        whiteLabelPerPageList.Add(new WhiteLabelPerPage(whiteLabelRecord, pageNumber));
                    }
                }

                if (documentId.IsNullOrEmpty())
                    throw new ArgumentNullException(nameof(documentId));

                whiteLabelsByDocumentIdList.Add(new WhiteLabelsByDocumentId(documentId, whiteLabelPerPageList));
                pdfDocument.Close();
                pdfFileStream.Close();

                return whiteLabelsByDocumentIdList;
            }
            catch (Exception e)
            {
                pdfDocument.Close();
                pdfFileStream.Close();
                var message = $"Page number:{pageNumber} - Read text:'{whiteLabelText}'";
                throw new Exception(message, e);
            }
        }

        private static void SplitPdf(string inputFilePath, string outputFilePath, int pageFrom, int pageTo)
        {
            using var pdfDoc = new iText.Kernel.Pdf.PdfDocument(new iText.Kernel.Pdf.PdfReader(inputFilePath));

            // Write current page to disk.
            using var writer = new iText.Kernel.Pdf.PdfWriter(outputFilePath);
            using var pdf = new iText.Kernel.Pdf.PdfDocument(writer);
            pdfDoc.CopyPagesTo(pageFrom: pageFrom, pageTo: pageTo, toDocument: pdf, insertBeforePage: 1);
        }

        private static WhiteLabel SplitTextFromString(string text, string startLabel = "", string separator = ":")
        {
            if (text.IsNullOrEmpty())
                throw new ArgumentNullException(nameof(text));

            var customerInfoArray = text.Split(startLabel, StringSplitOptions.RemoveEmptyEntries);

            // When program gets to the splitting stage in the pdf, it is read twice, so we only take the first one.
            var info = customerInfoArray[0].Split(separator, StringSplitOptions.RemoveEmptyEntries);

            // Except for startLabel, it should contain 7 parameters including endLabel.
            if (info.Length != 7)
                throw new MissingFieldException($"The white label text does not include some fields!");

            return new WhiteLabel(SLAName: info[0], PageType: info[1], Id: info[2], CountryCode: info[3]);
        }

        private async Task CreateResultPdf(string inputPdfFilePath, string outputFolderPath, List<WhiteLabelsByDocumentId> whiteLabelsByDocumentIdList, int uploadedFileId)
        {
            using var pdfFileStream = new FileStream(inputPdfFilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            var pdfDocument = new iText.Kernel.Pdf.PdfDocument(new iText.Kernel.Pdf.PdfReader(pdfFileStream));

            try
            {
                foreach (var whiteLabelsByDocumentId in whiteLabelsByDocumentIdList)
                {
                    var documentId = whiteLabelsByDocumentId.DocumentId;
                    var whiteLabelPerPageList = whiteLabelsByDocumentId.WhiteLabelPerPageList;

                    // If there is a previously created SplittedFile with the same DocumnetId, document splitting is canceled.
                    var sf = await _unitOfWork.SplittedFile.GetExecutedByDocumentIdAsync(documentId, true).ConfigureAwait(false);
                    if (sf != null)
                        throw new Exception($"SplittedFileId:'{sf.Id}' - DocumentId:'{documentId}' was created before...");

                    var splittedFile = new SplittedFile
                    {
                        UploadedFileId = uploadedFileId,
                        SlaName = whiteLabelPerPageList.First().WhiteLabel.SLAName,
                        PageType = whiteLabelPerPageList.First().WhiteLabel.PageType,
                        DocumentId = documentId,
                        CountryCode = whiteLabelPerPageList.First().WhiteLabel.CountryCode,
                        Status = AppConstant.SplittedFileStatusCreated
                    };

                    var outputDocumentFolder = Path.Combine(outputFolderPath, documentId);
                    FileHelper.CreateDirectory(outputDocumentFolder);

                    #region Split Pdf

                    var pageFrom = whiteLabelPerPageList.First().PageNumber;
                    var pageTo = whiteLabelPerPageList.Last().PageNumber;
                    var pdfPileName = $"{documentId}.pdf";
                    var outputFilePath = Path.Combine(outputDocumentFolder, pdfPileName);

                    splittedFile.FileName = pdfPileName;
                    splittedFile.FilePath = outputFilePath;
                    splittedFile.PageNumbersInBasePdf = string.Join(",", whiteLabelPerPageList.Select(s => s.PageNumber).ToArray());
                    await _unitOfWork.SplittedFile.CreateAsync(splittedFile).ConfigureAwait(false);

                    try
                    {
                        SplitPdf(inputPdfFilePath, outputFilePath, pageFrom, pageTo);
                    }
                    catch (Exception e)
                    {
                        splittedFile.FailedReason = e.Message;
                        splittedFile.Status = AppConstant.SplittedFileStatusFailed;
                        await _unitOfWork.SplittedFile.UpdateAsync(splittedFile).ConfigureAwait(false);
                        throw e;
                    }

                    splittedFile.Status = AppConstant.SplittedFileStatusSplitted;
                    await _unitOfWork.SplittedFile.UpdateAsync(splittedFile).ConfigureAwait(false);

                    #endregion Split Pdf
                }

                pdfDocument.Close();
                pdfFileStream.Close();
            }
            catch (Exception e)
            {
                pdfDocument.Close();
                pdfFileStream.Close();
                throw e;
            }
        }

        #endregion                
    }
}
