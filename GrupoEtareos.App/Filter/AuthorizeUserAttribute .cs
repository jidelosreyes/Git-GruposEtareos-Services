using GruposEtareos.BI;
using Microsoft.Owin.Security;
using System;
using System.Collections.Specialized;
using System.Linq;
using System.Security.Claims;
using System.Web;
using System.Web.Mvc;

namespace GrupoEtareos.App.Filter
{    
    public class AuthorizeUserAttribute : AuthorizeAttribute
    {
        private readonly GruposEtareosEntities db = new GruposEtareosEntities();

        public AuthorizeUserAttribute()
        {
            View = "AccesoDenegado";
            Master = string.Empty;
        }

        public string View { get; set; }
        public string Master { get; set; }

        public override void OnAuthorization(AuthorizationContext filterContext)
        {
            base.OnAuthorization(filterContext);
            CheckIfUserIsAuthenticated(filterContext);
        }

        private void CheckIfUserIsAuthenticated(AuthorizationContext filterContext)
        {
            var user = filterContext.HttpContext.User.Identity;
            
            if (!AuthorizeCore(filterContext.HttpContext))
            {
                if (string.IsNullOrEmpty(View))
                    return;
                var result = new ViewResult { ViewName = View, MasterName = Master };
                filterContext.Result = result;
            }
        }
        
        protected override bool AuthorizeCore(HttpContextBase httpContextBase)
        {
            var vAutorize = base.AuthorizeCore(httpContextBase);

            if (!vAutorize)
            {
                var CODUSUARIO = "";
                var PW = "";
                if (httpContextBase.Request.QueryString.Count == 0)
                {
                    if (httpContextBase.Request.UrlReferrer != null)
                    {
                        NameValueCollection nameValueCollection = HttpUtility.ParseQueryString(httpContextBase.Request.UrlReferrer.Query);
                        if (nameValueCollection.Count > 0)
                        {
                            var base64EncodedBytes = System.Convert.FromBase64String(nameValueCollection["paramView"]);
                            var strUrl = System.Text.Encoding.UTF8.GetString(base64EncodedBytes);
                            var queryStringUrl = EncriptarClaves.clsEncriptarClases.Decrypt(strUrl);

                            nameValueCollection = HttpUtility.ParseQueryString(queryStringUrl);
                            CODUSUARIO = nameValueCollection["CODUSUARIO"];
                            PW = nameValueCollection["CV"];
                        }
                    }
                }
                else
                {
                    //if (httpContextBase.Request.QueryString["CODUSUARIO"] != null)
                    if (httpContextBase.Request.QueryString["paramView"] != null)
                    {
                        var base64EncodedBytes = System.Convert.FromBase64String(httpContextBase.Request.QueryString["paramView"]);
                        var strUrl = System.Text.Encoding.UTF8.GetString(base64EncodedBytes);
                        var queryStringUrl = EncriptarClaves.clsEncriptarClases.Decrypt(strUrl);

                        NameValueCollection nameValueCollection = HttpUtility.ParseQueryString(queryStringUrl);
                        //CODUSUARIO = httpContextBase.Request.QueryString["CODUSUARIO"].ToString();
                        //PW = httpContextBase.Request.QueryString["CV"].ToString();
                        CODUSUARIO = nameValueCollection["CODUSUARIO"].ToString();
                        PW = nameValueCollection["CV"].ToString();
                    }
                }

                if (db.GN_USUARIO_SISTEMA.FirstOrDefault(d => d.COD_USUARIO == CODUSUARIO && d.CLAVE_USUARIO == PW) != null)
                {
                    var identity = new ClaimsIdentity("ApplicationCookie", ClaimsIdentity.DefaultNameClaimType, ClaimsIdentity.DefaultRoleClaimType);
                    identity.AddClaim(new Claim(ClaimTypes.Name, CODUSUARIO));
                    identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, CODUSUARIO));

                    IAuthenticationManager authenticationManager = httpContextBase.GetOwinContext().Authentication;
                    authenticationManager.SignOut("ApplicationCookie");
                    authenticationManager.SignIn(new AuthenticationProperties() { IsPersistent = false, ExpiresUtc = DateTime.Now.AddMinutes(2) }, identity);
                    return true;
                }
            }

            var user = httpContextBase.User.Identity;

            if (user.IsAuthenticated)
            {
                return true;
            }

            return false;
        }
    }
}