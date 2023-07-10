namespace pdf_page_splitter.Infrastructure
{
    public static class AppConstant
    {
        public const string UploadedFileStatusExecuting = "EXECUTING";
        public const string UploadedFileStatusExecuted = "EXECUTED";
        public const string UploadedFileStatusFailed = "FAILED";

        public const string SplittedFileStatusCreated = "CREATED";
        public const string SplittedFileStatusSplitted = "SPLITTED";
        public const string SplittedFileStatusFailed = "FAILED";
    }

    public class AppSettings
    {
        public FoldersSection Folders { get; set; }
        public LogDnaSection LogDNA { get; set; }
    }

    public class FoldersSection
    {
        public string InputFilesPath { get; set; }
        public string OutputFilesPath { get; set; }
        public string ArchivedFilesPath { get; set; }
        public string RejectedFilesPath { get; set; }
    }

    public class LogDnaSection
    {
        public string ApiKey { get; set; }
    }
}
