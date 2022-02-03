namespace FileExtractor.Utils
{
    internal interface ILogger<T>
    {
        void Information(string messageTemplate);
        void Information(string messageTemplate, params object[] propertyValues);

        void Warning(string messageTemplate);
        void Warning(string messageTemplate, params object[] propertyValues);

        void Error(Exception exception, string messageTemplate, params object[] propertyValues);
    }
}