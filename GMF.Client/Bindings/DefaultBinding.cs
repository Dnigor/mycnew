using System.ServiceModel;

namespace GMF.Client.Bindings
{
    public class DefaultBinding : BasicHttpBinding
    {
        public DefaultBinding() : base(BasicHttpSecurityMode.None)
        {
            this.Name                         = "DefaultBinding";
            this.Namespace                    = "http://www.marykay.com/";
            this.TransferMode                 = TransferMode.Buffered;
            this.MaxReceivedMessageSize       = 1048576;
            this.MaxBufferSize                = 1048576;
            this.ReaderQuotas.MaxBytesPerRead = 1048576;
            this.ReaderQuotas.MaxArrayLength  = 1000;
        }
    }
}
