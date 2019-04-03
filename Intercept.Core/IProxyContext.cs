using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading.Tasks;

namespace Intercept.Core
{
    public interface IProxyContext
    {
        /// <summary>
        /// 方法拦截处理函数
        /// </summary>
        /// <param name="call">方法调用消息</param>
        /// <param name="returnObject">方法调用返回消息</param>
        /// <returns>是否执行方法</returns>
        bool ProcessInvoke(IMethodCallMessage call, ref object returnObject);

        /// <summary>
        /// 方法开始执行
        /// </summary>
        /// <param name="msg">方法调用消息</param>
        void OnMethodExecuting(IMethodCallMessage msg);

        /// <summary>
        /// 方法执行完成
        /// </summary>
        /// <param name="msg">方法调用消息</param>
        /// <param name="returnMessage">方法调用返回消息</param>
        void OnMethodExecuted(IMethodCallMessage msg, IMethodReturnMessage returnMessage);
    }

    public abstract class ProxyContextAttribute : Attribute, IProxyContext
    {
        public virtual void OnMethodExecuted(IMethodCallMessage msg, IMethodReturnMessage returnMessage) { }

        public virtual void OnMethodExecuting(IMethodCallMessage msg) { }

        public abstract bool ProcessInvoke(IMethodCallMessage call, ref object returnObject);
    }
}
