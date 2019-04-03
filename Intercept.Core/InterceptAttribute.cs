using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Proxies;
using System.Text;
using System.Threading.Tasks;

namespace Intercept.Core
{
    public class InterceptAttribute : ProxyAttribute
    {
        public override MarshalByRefObject CreateInstance(Type serverType)
        {
            //未初始化的实例的默认透明代理
            MarshalByRefObject target = base.CreateInstance(serverType); //得到位初始化的实例（ctor未执行）
            //得到自定义的真实代理
            InterceptProxyBase rp = new InterceptProxy(target, serverType);
            return (MarshalByRefObject)rp.GetTransparentProxy();
        }
    }
}
