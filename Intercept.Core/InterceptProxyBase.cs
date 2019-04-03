using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Activation;
using System.Runtime.Remoting.Messaging;
using System.Runtime.Remoting.Proxies;
using System.Runtime.Remoting.Services;
using System.Text;
using System.Threading.Tasks;

namespace Intercept.Core
{
    /// <summary>
    /// 方法拦截
    /// </summary>
    public abstract class InterceptProxyBase : RealProxy
    {
        protected readonly MarshalByRefObject target;

        //public List<Action<IMethodCallMessage>> OnMethodExecuting = new List<Action<IMethodCallMessage>>();
        //public List<Action<IMethodCallMessage, IMethodReturnMessage>> OnMethodExecuted = new List<Action<IMethodCallMessage, IMethodReturnMessage>>();

        public InterceptProxyBase(MarshalByRefObject obj, Type type)
            : base(type)
        {
            target = obj;
        }

        public override IMessage Invoke(IMessage msg)
        {
            IMethodCallMessage call = (IMethodCallMessage)msg;
            IConstructionCallMessage ctor = call as IConstructionCallMessage;
            List<Action<IMethodCallMessage>> OnMethodExecuting = new List<Action<IMethodCallMessage>>();
            List<Action<IMethodCallMessage, IMethodReturnMessage>> OnMethodExecuted = new List<Action<IMethodCallMessage, IMethodReturnMessage>>();

            IMethodReturnMessage returnMessage = null;

            if (ctor != null)
            {
                //构造函数
                //获取最底层的默认真实代理
                RealProxy default_proxy = RemotingServices.GetRealProxy(this.target);
                default_proxy.InitializeServerObject(ctor);
                MarshalByRefObject tp = (MarshalByRefObject)this.GetTransparentProxy(); //自定义的透明代理 this
                returnMessage = EnterpriseServicesHelper.CreateConstructionReturnMessage(ctor, tp);
            }
            else
            {
                //方法
                object returnObject = null;
                if (!Intercept(call, ref returnObject,ref OnMethodExecuting,ref OnMethodExecuted))
                {
                    if (returnObject is IMethodReturnMessage)
                    {
                        returnMessage = (IMethodReturnMessage)returnObject;
                    }
                    else
                    {
                        if (returnObject == null)
                        {
                            var method = (MethodInfo)call.MethodBase;
                            var returnType = method.ReturnType;
                            if (returnType != typeof(void))
                            {
                                returnObject = returnType.IsValueType ? Activator.CreateInstance(returnType) : null;
                            }
                        }
                        returnMessage = new ReturnMessage(returnObject, call.Args, call.ArgCount, call.LogicalCallContext, call);
                    }
                }
                else
                {
                    OnMethodExecuting.ForEach((method) => { method(call); });
                    OnMethodExecuting.Clear();
                    try
                    {
                        returnMessage = RemotingServices.ExecuteMessage(this.target, call);
                    }
                    catch (Exception e)
                    {
                        returnMessage = new ReturnMessage(e, call);
                    }
                    OnMethodExecuted.ForEach((method) => { method(call, returnMessage); });
                    OnMethodExecuted.Clear();
                }
            }
            return returnMessage;
        }

        /// <summary>
        /// 方法拦截处理函数
        /// </summary>
        /// <param name="call">方法调用消息</param>
        /// <param name="returnObject">方法调用返回消息</param>
        /// <returns>是否执行方法</returns>
        protected virtual bool Intercept(IMethodCallMessage call, ref object returnObject,ref List<Action<IMethodCallMessage>> OnMethodExecuting,ref List<Action<IMethodCallMessage, IMethodReturnMessage>> OnMethodExecuted)
        {
            returnObject = null;
            return true;
        }
    }
}
