using Intercept.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Remoting.Messaging;
using System.Threading;
using Newtonsoft.Json;

namespace Intercept.Console
{
    class Program
    {
        static void Main(string[] args)
        {
            ProxyTest test = new ProxyTest();
            #region CacheTest
            //System.Console.WriteLine(test.Calc(1, 1));
            //System.Console.WriteLine(test.Calc(1, 1));
            //System.Console.WriteLine(test.Calc(1, 2));
            //System.Console.WriteLine(test.Calc(1, 2));
            #endregion

            #region RoleTest
            //System.Console.WriteLine(test.Calc(1, 1, 1));
            //test.Login("Admin", "Admin", "Admin");
            //System.Console.WriteLine(test.Calc(2, 2, 2));
            #endregion
        }
    }

    #region CacheIMPL
    class CacheService
    {
        static Dictionary<object, object> cache = new Dictionary<object, object>();
        public static object Cache(object key)
        {
            if (cache.ContainsKey(key))
            {
                return cache[key];
            }
            else
            {
                return null;
            }
        }

        public static void Cache(object key, object value)
        {
            cache[key] = value;
        }
    }

    class CacheAttribute : ProxyContextAttribute
    {
        bool isCache = false;
        public override bool ProcessInvoke(IMethodCallMessage call, ref object returnObject)
        {
            var key = HashCode(call);
            returnObject = CacheService.Cache(key);
            if (returnObject != null)
            {
                isCache = true;
                return false;
            }
            return true;
        }

        public override void OnMethodExecuting(IMethodCallMessage msg)
        {
            base.OnMethodExecuting(msg);
        }

        public override void OnMethodExecuted(IMethodCallMessage msg, IMethodReturnMessage returnMessage)
        {
            if (!isCache)
            {
                var key = HashCode(msg);
                CacheService.Cache(key, returnMessage);
            }
        }

        private int HashCode(IMethodCallMessage msg)
        {
            string args = JsonConvert.SerializeObject(msg.InArgs);
            string name = $"{msg.MethodBase.DeclaringType.FullName}.{msg.MethodBase.Name}({args})";
            int hash = name.GetHashCode();
            return hash;
        }
    }
    #endregion

    #region RoleIMPL
    class UserInfo
    {
        public string User { get; set; }
        public string Pwd { get; set; }
        public string Role { get; set; } = "Admin";
    }
    class UserLoginInfo
    {
        public static UserInfo User = null;
        public static bool Login(string username, string pwd, string role)
        {
            if ("Admin".Equals(username) && "Admin".Equals(pwd))
            {
                User = new UserInfo() { User = username, Pwd = pwd, Role = role };
                return true;
            }
            return false;
        }
    }
    class RoleAttribute : ProxyContextAttribute
    {
        string RoleName;
        public RoleAttribute(string roleName)
        {
            RoleName = roleName;
        }

        public override bool ProcessInvoke(IMethodCallMessage call, ref object returnObject)
        {
            var user = UserLoginInfo.User;
            if (user != null)
            {
                if (user.Role.Equals(RoleName))
                {
                    return true;
                }
            }
            return false;
        }
    }
    #endregion

    [Intercept]
    class ProxyTest : ContextBoundObject
    {
        /// <summary>
        /// 模拟耗时操作
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        [Cache]
        public int Calc(int a, int b)
        {
            Thread.Sleep(5000);
            return a + b;
        }

        public void Login(string username, string pwd, string role)
        {
            UserLoginInfo.Login(username, pwd, role);
        }

        /// <summary>
        /// 需要权限的操作
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="c"></param>
        /// <returns></returns>
        [Role("Admin")]
        public int Calc(int a, int b, int c)
        {
            return a + b + c;
        }
    }
}
