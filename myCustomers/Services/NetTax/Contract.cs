﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.18047
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

[assembly: System.Runtime.Serialization.ContractNamespaceAttribute("http://tempuri.org/", ClrNamespace="NetTax")]

namespace myCustomers.Services.NetTax
{
    using System.Runtime.Serialization;
    
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Runtime.Serialization", "4.0.0.0")]
    [System.Runtime.Serialization.DataContractAttribute(Name="ScrubResults", Namespace="http://tempuri.org/")]
    public partial class ScrubResults : object, System.Runtime.Serialization.IExtensibleDataObject
    {
        
        private System.Runtime.Serialization.ExtensionDataObject extensionDataField;
        
        private string AddressField;
        
        private string CityField;
        
        private string ShortCityField;
        
        private string StateField;
        
        private string ZipField;
        
        private string ZipPlus4Field;
        
        private string CountyField;
        
        private string MessageField;
        
        public System.Runtime.Serialization.ExtensionDataObject ExtensionData
        {
            get
            {
                return this.extensionDataField;
            }
            set
            {
                this.extensionDataField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute(EmitDefaultValue=false)]
        public string Address
        {
            get
            {
                return this.AddressField;
            }
            set
            {
                this.AddressField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute(EmitDefaultValue=false)]
        public string City
        {
            get
            {
                return this.CityField;
            }
            set
            {
                this.CityField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute(EmitDefaultValue=false)]
        public string ShortCity
        {
            get
            {
                return this.ShortCityField;
            }
            set
            {
                this.ShortCityField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute(EmitDefaultValue=false)]
        public string State
        {
            get
            {
                return this.StateField;
            }
            set
            {
                this.StateField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute(EmitDefaultValue=false)]
        public string Zip
        {
            get
            {
                return this.ZipField;
            }
            set
            {
                this.ZipField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute(EmitDefaultValue=false)]
        public string ZipPlus4
        {
            get
            {
                return this.ZipPlus4Field;
            }
            set
            {
                this.ZipPlus4Field = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute(EmitDefaultValue=false, Order=6)]
        public string County
        {
            get
            {
                return this.CountyField;
            }
            set
            {
                this.CountyField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute(EmitDefaultValue=false, Order=7)]
        public string Message
        {
            get
            {
                return this.MessageField;
            }
            set
            {
                this.MessageField = value;
            }
        }
    }
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    [System.ServiceModel.ServiceContractAttribute(ConfigurationName="Code1ServiceSoap")]
    public interface ICode1Service
    {
        
        // CODEGEN: Generating message contract since element name Address from namespace http://tempuri.org/ is not marked nillable
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/ScrubAddress")]
        ScrubAddressResponse ScrubAddress(ScrubAddressRequest request);
        
        // CODEGEN: Generating message contract since element name Address from namespace http://tempuri.org/ is not marked nillable
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/ScrubAddressForOrderEntry")]
        ScrubAddressForOrderEntryResponse ScrubAddressForOrderEntry(ScrubAddressForOrderEntryRequest request);
        
        // CODEGEN: Generating message contract since element name Address from namespace http://tempuri.org/ is not marked nillable
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/ValidateMailingAddress")]
        ValidateMailingAddressResponse ValidateMailingAddress(ValidateMailingAddressRequest request);
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
    [System.ServiceModel.MessageContractAttribute(IsWrapped=false)]
    public partial class ScrubAddressRequest
    {
        
        [System.ServiceModel.MessageBodyMemberAttribute(Name="ScrubAddress", Namespace="http://tempuri.org/", Order=0)]
        public ScrubAddressRequestBody Body;
        
        public ScrubAddressRequest()
        {
        }
        
        public ScrubAddressRequest(ScrubAddressRequestBody Body)
        {
            this.Body = Body;
        }
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
    [System.Runtime.Serialization.DataContractAttribute(Namespace="http://tempuri.org/")]
    public partial class ScrubAddressRequestBody
    {
        
        [System.Runtime.Serialization.DataMemberAttribute(EmitDefaultValue=false, Order=0)]
        public string Address;
        
        [System.Runtime.Serialization.DataMemberAttribute(EmitDefaultValue=false, Order=1)]
        public string City;
        
        [System.Runtime.Serialization.DataMemberAttribute(EmitDefaultValue=false, Order=2)]
        public string State;
        
        [System.Runtime.Serialization.DataMemberAttribute(EmitDefaultValue=false, Order=3)]
        public string Zip;
        
        public ScrubAddressRequestBody()
        {
        }
        
        public ScrubAddressRequestBody(string Address, string City, string State, string Zip)
        {
            this.Address = Address;
            this.City = City;
            this.State = State;
            this.Zip = Zip;
        }
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
    [System.ServiceModel.MessageContractAttribute(IsWrapped=false)]
    public partial class ScrubAddressResponse
    {
        
        [System.ServiceModel.MessageBodyMemberAttribute(Name="ScrubAddressResponse", Namespace="http://tempuri.org/", Order=0)]
        public ScrubAddressResponseBody Body;
        
        public ScrubAddressResponse()
        {
        }
        
        public ScrubAddressResponse(ScrubAddressResponseBody Body)
        {
            this.Body = Body;
        }
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
    [System.Runtime.Serialization.DataContractAttribute(Namespace="http://tempuri.org/")]
    public partial class ScrubAddressResponseBody
    {
        
        [System.Runtime.Serialization.DataMemberAttribute(EmitDefaultValue=false, Order=0)]
        public ScrubResults ScrubAddressResult;
        
        public ScrubAddressResponseBody()
        {
        }
        
        public ScrubAddressResponseBody(ScrubResults ScrubAddressResult)
        {
            this.ScrubAddressResult = ScrubAddressResult;
        }
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
    [System.ServiceModel.MessageContractAttribute(IsWrapped=false)]
    public partial class ScrubAddressForOrderEntryRequest
    {
        
        [System.ServiceModel.MessageBodyMemberAttribute(Name="ScrubAddressForOrderEntry", Namespace="http://tempuri.org/", Order=0)]
        public ScrubAddressForOrderEntryRequestBody Body;
        
        public ScrubAddressForOrderEntryRequest()
        {
        }
        
        public ScrubAddressForOrderEntryRequest(ScrubAddressForOrderEntryRequestBody Body)
        {
            this.Body = Body;
        }
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
    [System.Runtime.Serialization.DataContractAttribute(Namespace="http://tempuri.org/")]
    public partial class ScrubAddressForOrderEntryRequestBody
    {
        
        [System.Runtime.Serialization.DataMemberAttribute(EmitDefaultValue=false, Order=0)]
        public string Address;
        
        [System.Runtime.Serialization.DataMemberAttribute(EmitDefaultValue=false, Order=1)]
        public string City;
        
        [System.Runtime.Serialization.DataMemberAttribute(EmitDefaultValue=false, Order=2)]
        public string State;
        
        [System.Runtime.Serialization.DataMemberAttribute(EmitDefaultValue=false, Order=3)]
        public string Zip;
        
        public ScrubAddressForOrderEntryRequestBody()
        {
        }
        
        public ScrubAddressForOrderEntryRequestBody(string Address, string City, string State, string Zip)
        {
            this.Address = Address;
            this.City = City;
            this.State = State;
            this.Zip = Zip;
        }
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
    [System.ServiceModel.MessageContractAttribute(IsWrapped=false)]
    public partial class ScrubAddressForOrderEntryResponse
    {
        
        [System.ServiceModel.MessageBodyMemberAttribute(Name="ScrubAddressForOrderEntryResponse", Namespace="http://tempuri.org/", Order=0)]
        public ScrubAddressForOrderEntryResponseBody Body;
        
        public ScrubAddressForOrderEntryResponse()
        {
        }
        
        public ScrubAddressForOrderEntryResponse(ScrubAddressForOrderEntryResponseBody Body)
        {
            this.Body = Body;
        }
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
    [System.Runtime.Serialization.DataContractAttribute(Namespace="http://tempuri.org/")]
    public partial class ScrubAddressForOrderEntryResponseBody
    {
        
        [System.Runtime.Serialization.DataMemberAttribute(EmitDefaultValue=false, Order=0)]
        public ScrubResults ScrubAddressForOrderEntryResult;
        
        public ScrubAddressForOrderEntryResponseBody()
        {
        }
        
        public ScrubAddressForOrderEntryResponseBody(ScrubResults ScrubAddressForOrderEntryResult)
        {
            this.ScrubAddressForOrderEntryResult = ScrubAddressForOrderEntryResult;
        }
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
    [System.ServiceModel.MessageContractAttribute(IsWrapped=false)]
    public partial class ValidateMailingAddressRequest
    {
        
        [System.ServiceModel.MessageBodyMemberAttribute(Name="ValidateMailingAddress", Namespace="http://tempuri.org/", Order=0)]
        public ValidateMailingAddressRequestBody Body;
        
        public ValidateMailingAddressRequest()
        {
        }
        
        public ValidateMailingAddressRequest(ValidateMailingAddressRequestBody Body)
        {
            this.Body = Body;
        }
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
    [System.Runtime.Serialization.DataContractAttribute(Namespace="http://tempuri.org/")]
    public partial class ValidateMailingAddressRequestBody
    {
        
        [System.Runtime.Serialization.DataMemberAttribute(EmitDefaultValue=false, Order=0)]
        public string Address;
        
        [System.Runtime.Serialization.DataMemberAttribute(EmitDefaultValue=false, Order=1)]
        public string City;
        
        [System.Runtime.Serialization.DataMemberAttribute(EmitDefaultValue=false, Order=2)]
        public string State;
        
        [System.Runtime.Serialization.DataMemberAttribute(EmitDefaultValue=false, Order=3)]
        public string Zip;
        
        public ValidateMailingAddressRequestBody()
        {
        }
        
        public ValidateMailingAddressRequestBody(string Address, string City, string State, string Zip)
        {
            this.Address = Address;
            this.City = City;
            this.State = State;
            this.Zip = Zip;
        }
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
    [System.ServiceModel.MessageContractAttribute(IsWrapped=false)]
    public partial class ValidateMailingAddressResponse
    {
        
        [System.ServiceModel.MessageBodyMemberAttribute(Name="ValidateMailingAddressResponse", Namespace="http://tempuri.org/", Order=0)]
        public ValidateMailingAddressResponseBody Body;
        
        public ValidateMailingAddressResponse()
        {
        }
        
        public ValidateMailingAddressResponse(ValidateMailingAddressResponseBody Body)
        {
            this.Body = Body;
        }
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
    [System.Runtime.Serialization.DataContractAttribute(Namespace="http://tempuri.org/")]
    public partial class ValidateMailingAddressResponseBody
    {
        
        [System.Runtime.Serialization.DataMemberAttribute(EmitDefaultValue=false, Order=0)]
        public ScrubResults ValidateMailingAddressResult;
        
        public ValidateMailingAddressResponseBody()
        {
        }
        
        public ValidateMailingAddressResponseBody(ScrubResults ValidateMailingAddressResult)
        {
            this.ValidateMailingAddressResult = ValidateMailingAddressResult;
        }
    }
}

