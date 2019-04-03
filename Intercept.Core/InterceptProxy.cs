using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading.Tasks;

namespace Intercept.Core
{
    public class InterceptProxy : InterceptProxyBase
    {
        public InterceptProxy(MarshalByRefObject obj, Type type) : base(obj, type)
        {
        }

        protected override bool Intercept(IMethodCallMessage call, ref object returnObject, ref List<Action<IMethodCallMessage>> OnMethodExecuting, ref List<Action<IMethodCallMessage, IMethodReturnMessage>> OnMethodExecuted)
        {
            //获取方法标记的已继承IProxyContext的Attribute
            var typeAttributes = call.MethodBase.GetCustomAttributes(typeof(IProxyContext), false).Select(x => x as IProxyContext).ToList();
            bool invoke = true;
            foreach (var attribute in typeAttributes)
            {
                OnMethodExecuting.Add(attribute.OnMethodExecuting);
                OnMethodExecuted.Add(attribute.OnMethodExecuted);
                invoke = attribute.ProcessInvoke(call, ref returnObject);
                if (!invoke)
                {
                    break;
                }
            }
            return invoke;
        }
    }
}
