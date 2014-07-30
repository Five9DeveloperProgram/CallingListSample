/* 
Copyright (C) 2014 Five9, Inc. All rights reserved.

www.five9.com/legal#terms

Software is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES 
 OR CONDITIONS OF ANY KIND, either express or implied.
*/
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.ServiceModel.Dispatcher;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;

namespace Five9CommonLib
{
    /// <summary>
    /// A class that implements IClientMessageInspector so that an "Authorization" HTTP Header
    /// may be inserted into an HTTP message before it is sent.  This class is primarily used
    /// for the Admin and Supervisor web service clients because those web services require
    /// the Authorization HTTP header to provide security on the web service.
    /// </summary>
    public class AuthHeaderInserter : IClientMessageInspector
    {
        public string Username
        {
            get;
            set;
        }

        public string Password
        {
            get;
            set;
        }

        #region IClientMessageInspector Members

        public void AfterReceiveReply(ref System.ServiceModel.Channels.Message reply, object correlationState)
        {
            //Debug.WriteLine(reply);
        }

        /// <summary>
        /// Before sending an HTTP request this method inserts the Authorization HTTP header using Basic security
        /// with the username:password base64 encoded.
        /// </summary>
        /// <param name="request"></param>
        /// <param name="channel"></param>
        /// <returns></returns>
        public object BeforeSendRequest(ref System.ServiceModel.Channels.Message request, System.ServiceModel.IClientChannel channel)
        {
            //Get the HttpRequestMessage property from the message
            HttpRequestMessageProperty httpreq = null;

            try
            {
                httpreq = request.Properties[HttpRequestMessageProperty.Name] as HttpRequestMessageProperty;
            }
            catch //(Exception exc)
            {
            }

            if (httpreq == null)
            {
                httpreq = new HttpRequestMessageProperty();
                request.Properties.Add(HttpRequestMessageProperty.Name, httpreq);
            }

            byte[] authbytes = Encoding.UTF8.GetBytes(string.Concat(Username, ":", Password));
            string base64 = Convert.ToBase64String(authbytes);
            string authorization = string.Concat("Basic ", base64);

            httpreq.Headers["Authorization"] = authorization;

            return null;
        }

        #endregion
    }

    /// <summary>
    /// A class used to add the AuthHeaderInserter into the endpoints behavior collection.
    /// </summary>
    public class AuthHeaderBehavior : IEndpointBehavior
    {
        private AuthHeaderInserter authHeaderInserter;

        public AuthHeaderBehavior(AuthHeaderInserter headerInserter)
        {
            authHeaderInserter = headerInserter;
        }

        #region IEndpointBehavior Members

        public void AddBindingParameters(ServiceEndpoint endpoint, BindingParameterCollection bindingParameters)
        {

        }

        /// <summary>
        /// Add the AuthHeaderInserter into the MessageInspector collection for the endpoint.
        /// </summary>
        /// <param name="endpoint"></param>
        /// <param name="clientRuntime"></param>
        public void ApplyClientBehavior(ServiceEndpoint endpoint, ClientRuntime clientRuntime)
        {
            clientRuntime.MessageInspectors.Add(authHeaderInserter);
            Debug.WriteLine("Added AuthHeaderInserter into MessageIspector collection for endpoint: " + endpoint.Address.ToString());
        }

        public void ApplyDispatchBehavior(ServiceEndpoint endpoint, EndpointDispatcher endpointDispatcher)
        {
        }

        public void Validate(ServiceEndpoint endpoint)
        {

        }

        #endregion
    }
}
