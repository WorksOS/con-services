using System;

namespace Utilities.Logging
{
    public interface ILoggingService
    {
		void CreateLogger<T>();
		void CreateLogger(Type type);
        void Debug(string message, string classMethod);
        void Info(string message, string classMethod);
        void Error(string message, string classMethod, Exception ex);
		void Error<T>(string message, string classMethod, Exception ex, T errors);
		void Fatal(string message, string classMethod);
        void Warn(string message, string classMethod);
    }
}
