using System;
using System.Net;
using SafeVault.Exceptions;
using SafeVault.Transport.Exceptions;
using ApplicationException = System.ApplicationException;

namespace SafeVault.Transport
{
    public class HttpServiceChannel : SecureChannel
    {
        private HttpWebRequest _httpRequest;
        private HttpWebResponse _httpResponse;

        private EncryptedStream _requestStream;
        private EncryptedStream _responseStream;
        protected override EncryptedStream ReadStream 
        {
            get
            {
                if (_httpResponse == null)
                    throw new ApplicationException("Opertion is not avaiable. Post request first");

                return _responseStream;
            }
            set { throw new NotSupportedException(); }
        }
        protected override EncryptedStream WriteStream 
        {
            get
            {
                if (_httpResponse != null)
                    throw new ApplicationException("Opertion is not avaiable");

                return (_requestStream != null)
                    ? _requestStream
                    : (_requestStream = new EncryptedStream(_httpRequest.GetRequestStream()));
            }
            set { throw new NotSupportedException(); }
        }

        public HttpServiceChannel(Uri address)
        {
            _httpRequest = (HttpWebRequest)WebRequest.Create(address);
            _httpRequest.Method = "POST";

            _httpRequest.Proxy = WebRequest.GetSystemWebProxy();
            if (_httpRequest.Proxy != null)
                _httpRequest.Proxy.Credentials = CredentialCache.DefaultCredentials;

            _httpRequest.ContentType = "application/encrypted-data";
        }

        public void Post()
        {
            if (_requestStream != null)
                _requestStream.Flush();

            _requestStream?.Dispose();
            _requestStream = null;

            try
            {
                _httpResponse = (HttpWebResponse) _httpRequest.GetResponse();

                if (_httpResponse.ContentType != "application/encrypted-data")
                    throw new WebException("Invalid ContentType", WebExceptionStatus.ProtocolError);

                var responseStream = _httpResponse.GetResponseStream();
                _responseStream = (responseStream != null) 
                    ? new EncryptedStream(responseStream) 
                    : null;               
            }
            catch (WebException e)
            {
                _httpResponse = (HttpWebResponse) e.Response;
                if (_httpResponse != null)
                {
                    var responseStream = _httpResponse.GetResponseStream();
                    _requestStream = (responseStream != null) ? new EncryptedStream(responseStream) : null;

                    throw new HttpChannelException($"{_httpResponse.StatusCode}: {e.Message}")
                    {
                        StatusCode = (int) _httpResponse.StatusCode
                    };
                }
                throw new HttpChannelException($"{e.Status}: {e.Message}") {StatusCode = (int) e.Status};
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _requestStream?.Dispose();
                _requestStream = null;

                _responseStream?.Dispose();
                _responseStream = null;

                #if NETSTANDARD2_0
                _httpResponse?.Dispose();
                _httpResponse = null;
                #endif

                #if NETFX
                _httpResponse?.Close();
                _httpResponse = null;
                #endif

                _httpRequest = null;
            }
            base.Dispose(disposing);
        }
    }
}