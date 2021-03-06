﻿using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using BaristaLabs.ChromeDevTools.Runtime.Runtime;

namespace Tera.ChromeDevTools
{
    public class DynamicObjectResult : DynamicObject,IDisposable
    {
         private string objectId;
        private readonly ChromeSession session;

  
        public DynamicObjectResult(string objectId, ChromeSession session)
        {
            this.objectId = objectId;
            this.session = session;
        }

        internal static DynamicObjectResult Get(EvaluateCommandResponse result, ChromeSession session)
        {
            if (result.ExceptionDetails != null)
            {
                throw new ChromeRemoteException(result.ExceptionDetails);
            }
            return Get(result.Result, session);
        }
        internal static DynamicObjectResult Get(RemoteObject value, ChromeSession session)
        {
            if (value.Type == "undefined") return null;

           
           return new DynamicObjectResult(value.ObjectId, session); 
        }
        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            var properties = session.InspectObject(this.objectId).GetAwaiter().GetResult();
            if (properties.ExceptionDetails != null)
            {
                throw new ChromeRemoteException(properties.ExceptionDetails);
            }

            dynamic property = properties.Result.FirstOrDefault(p => p.Name == binder.Name);
            if (property == null)
            {
                property = properties.InternalProperties.FirstOrDefault(p => p.Name == binder.Name);

            }
            if (property != null)
            {
                result = property.Value.Value == null ? Get(property.Value, session) : property.Value.Value;
                return true;
            }
            result = null;
            return false;
        }
        ~DynamicObjectResult()
        {
            Dispose();
        }
        public void Dispose()
        {
            GC.SuppressFinalize(this);
            session.RemoveObject(objectId).GetAwaiter().GetResult();
        }
    }
}
