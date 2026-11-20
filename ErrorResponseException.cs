using Newtonsoft.Json;
using System;
using System.Net;
using System.Net.Http;

namespace Challenge.DataContracts
{
    public class ErrorResponseException : Exception
    {
        public ErrorResponseException(HttpStatusCode statusCode, ErrorResponse errorResponse)
            : base(errorResponse.ErrorName)
        {
            StatusCode = statusCode;
            ErrorName = errorResponse.ErrorName;
            ErrorLevel = errorResponse.ErrorLevel;
            ErrorDescription = errorResponse.ErrorDescription;
        }

        public ErrorResponseException(HttpStatusCode statusCode, string message)
            : base(message)
        {
            StatusCode = statusCode;
        }

        public HttpStatusCode StatusCode { get; }
        public string ErrorName { get; }
        public string ErrorLevel { get; }
        public string ErrorDescription { get; }

        public static ErrorResponseException ExtractFrom(HttpResponseMessage response)
        {
            string body = null;
            try
            {
                body = response.Content.ReadAsStringAsync().Result;
                var errorResponse = JsonConvert.DeserializeObject<ErrorResponse>(body);
                if (errorResponse != null && !string.IsNullOrEmpty(errorResponse.ErrorName))
                    return new ErrorResponseException(response.StatusCode, errorResponse);
            }
            catch (Exception)
            {
            }

            // Тело ответа не содержит описания ошибки: показываем статус и тело как есть
            var message = $"{(int)response.StatusCode} {response.ReasonPhrase}";
            if (!string.IsNullOrWhiteSpace(body))
                message += $": {body}";
            return new ErrorResponseException(response.StatusCode, message);
        }
    }
}
