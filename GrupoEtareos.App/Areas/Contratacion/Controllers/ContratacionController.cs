using GrupoEtareos.App.Filter;
using GruposEtareos.BI;
using GruposEtareos.BI.EntitiesAditional;
using Microsoft.AspNetCore.Cors;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;

namespace GrupoEtareos.App.Areas.Contratacion.Controllers
{
    /// <summary>
    /// AuthorizeUser es parametrizado para evitar que se haga uso del controlador sin haberse logueado con las credenciades de INTEGRA
    /// </summary>
    //[AuthorizeUser]
    [EnableCors(origins: "http://localhost:4200/", headers: "*", methods: "*")]
    public class ContratacionController : Controller
    {
        /// <summary>
        /// Variable para hacer uso de las entidades del modelo de datos
        /// </summary>
        private GruposEtareosEntities db = new GruposEtareosEntities();

        #region Constante de campos para la entidad PS_SERVICIOS
        private const string FieldEntitie = @"COD_SERVICIO
                                             ,COD_GRUPO
                                             ,COD_SUBGRUPO
                                             ,DESCR_SERVICIO
                                             ,ABREVIATURA
                                             ,COD_ENTIDAD
                                             ,COD_CLASIF_CONTABLE
                                             ,APLICA_ESPECIALIDAD
                                             ,COD_CONCEPTOC_EVENTO
                                             ,COD_CONCEPTOC_CAPITA
                                             ,COD_ENTIDADSP
                                             ,COMPLEJIDAD
                                             ,REGISTRO_SANITARIO
                                             ,COD_TIPO_SERVICIO
                                             ,ID_ESTADO_SERVICIOS
                                             ,CUM
                                             ,OBSERVACIONES
                                             ,FORMULA_MAGISTRAL
                                             ,VITAL_DISPONIBLE
                                             ,ID_TECNOLOGIA
                                             ,AUTORIZACION_RELACIONADA
                                             ,CODIGO_IUM
                                             ,NIVEL_AUTORIZACION
                                             ,EDAD_MINIMA
                                             ,EDAD_MAXIMA
                                             ,COD_SEXO
                                             ,UTILIZACION_MAXIMA
                                             ,PERIODO_UTILIZACION_MAXIMA
                                             ,PERIODO_TOPE_MAXIMO
                                             ,MAXIMOXORDEN
                                             ,DIAS_EXPIRACION
                                             ,AUTORIZACION_INICIAL
                                             ,DIAS_MAX_AUTORIZADOS
                                             ,VALOR_VARIABLE
                                             ,REVISION_DIAGNOSTICOS
                                             ,PROFESIONAL_AUTORIZA
                                             ,APLICA_COPAGO
                                             ,APLICA_CUOTAMODERADORA
                                             ,EXCLUIRVALIDACION";
        #endregion

        /// <summary>
        /// Se ejecuta al iniciar la opción de creación de servicios, es la primera funcion ejecutada al inciar el llamado al controlador
        /// </summary>
        /// <returns></returns>
        // GET: Contratacion/Contratacion
        public ActionResult Index()
        {
            var strCodigoServicio = "";
            var strCodigoSubGrupo = "";
            var strCodigoGrupo = "";
            var strCodigoEPS = "";
            var strCodigoRegional = "0";
            var psServicio = new PS_SERVICIOS();
            NameValueCollection nameValueCollection = null;

            if (HttpContext.Request.QueryString["paramView"] != null)
            {
                var base64EncodedBytes = Convert.FromBase64String(HttpContext.Request.QueryString["paramView"]);
                var strUrl = System.Text.Encoding.UTF8.GetString(base64EncodedBytes);
                var queryStringUrl = EncriptarClaves.clsEncriptarClases.Decrypt(strUrl);

                nameValueCollection = HttpUtility.ParseQueryString(queryStringUrl);

            }

            if (nameValueCollection != null && (nameValueCollection["strCodigoRegional"] != null || nameValueCollection["strCodigoEPS"] != null))
            {
                strCodigoEPS = nameValueCollection["strCodigoEPS"];
                strCodigoRegional = nameValueCollection["strCodigoRegional"];
            }

            ViewBag.strCodigoEPS = strCodigoEPS;
            ViewBag.strCodigoRegional = strCodigoRegional;

            if (nameValueCollection != null && nameValueCollection["strOpcion"] != null)
            {
                var strOpcion = nameValueCollection["strOpcion"];

                if (strOpcion.Contains("C?"))
                {
                    ViewBag.ViewBotonesConsulta = true;
                }
            }

            if (HttpContext.Request.UrlReferrer != null && HttpContext.Request.UrlReferrer.IsAbsoluteUri)
                ViewBag.UrlReferer = HttpContext.Request.UrlReferrer.AbsoluteUri;
            else if (nameValueCollection != null && nameValueCollection["strUrlReferrer"] != null)
            {
                ViewBag.UrlReferer = nameValueCollection["strUrlReferrer"];
            }

            if (nameValueCollection != null && (nameValueCollection["strCodigoServicio"] != null && nameValueCollection["strCodigoSubGrupo"] != null && nameValueCollection["strCodigoGrupo"] != null))
            {
                strCodigoServicio = nameValueCollection["strCodigoServicio"];
                strCodigoSubGrupo = nameValueCollection["strCodigoSubGrupo"];
                strCodigoGrupo = nameValueCollection["strCodigoGrupo"];

                ViewBag.Editar = true;

                psServicio = db.PS_SERVICIOS.FirstOrDefault(d => d.COD_SERVICIO == strCodigoServicio && d.COD_GRUPO == strCodigoGrupo && d.COD_SUBGRUPO == strCodigoSubGrupo);

                if (psServicio == null)
                {
                    ViewBag.Editar = false;
                    psServicio = new PS_SERVICIOS();
                }
            }
            else
            {
                ViewBag.Estados = new SelectList(new List<object> { new object { } }, "ID", "DECRIPCION");
                ViewBag.Editar = false;
            }

            ViewBag.strCodigoServicio = strCodigoServicio;
            ViewBag.strCodigoSubGrupo = strCodigoSubGrupo;
            ViewBag.strCodigoGrupo = strCodigoGrupo;

            if (nameValueCollection != null && nameValueCollection["Editar"] != null)
            {
                ViewBag.RedirectEdit = nameValueCollection["Editar"];
            } else if (HttpContext.Request.QueryString["Editar"] != null)
            {
                ViewBag.RedirectEdit = HttpContext.Request.QueryString["Editar"];

            }

            CargarListasFormulario(psServicio, ViewBag.Editar);
            return View(psServicio);
        }

        /// <summary>
        /// Se ejecuta cuando se envia el formulario con la opción de create, por la opción POST.
        /// </summary>
        /// <param name="pS_SERVICIOS">Indica la entidad a crear con los parametros para la creación del servicio.</param>
        /// <param name="ClasificacionServicio">Objeto string que contiene un JSON enviado desde la vista para la clasificación parametrizada en el servicio</param>
        /// <param name="MENSAJE_INFORMATIVO">Indica el mensaje informativo enviado desde la creación servicio</param>
        /// <param name="COD_PLAN">Indica el código del plan seleccionado cuando el servicio es NO POS</param>
        /// <param name="COD_EPS">Indica el código de la eps seleccionada cuando el servicio es NO POS</param>
        /// <param name="COD_REGIONAL">Indica el código de la regional seleccionada cuando el servicio es NO POS</param>
        /// <returns>Retorna la vista a ejecutar al momento de realizar la creación del servicio</returns>
        // POST: Contratacion/Contratacion/Create
        [HttpPost]
        //[ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = FieldEntitie)] PS_SERVICIOS pS_SERVICIOS, string ClasificacionServicio, string MENSAJE_INFORMATIVO, string COD_PLAN, string COD_EPS, string COD_REGIONAL)
        {
            //using (var tran = db.Database.BeginTransaction())
            {
                try
                {
                    ModelState["AUTORIZACION_INICIAL"].Errors.Clear();
                    ModelState["VALOR_VARIABLE"].Errors.Clear();
                    if (ModelState.IsValid)
                    {
                        pS_SERVICIOS.FECHA_CREACION = DateTime.Now;
                        pS_SERVICIOS.AUTORIZACION_INICIAL = "S";
                        pS_SERVICIOS.VALOR_VARIABLE = "S";
                        //pS_SERVICIOS.APLICA_COPAGO = pS_SERVICIOS.APLICA_COPAGO == null ? false : pS_SERVICIOS.APLICA_COPAGO;
                        //pS_SERVICIOS.APLICA_CUOTAMODERADORA = pS_SERVICIOS.APLICA_CUOTAMODERADORA == null ? false : pS_SERVICIOS.APLICA_CUOTAMODERADORA;
                        if (!db.PS_SERVICIOS.Where(d => d.COD_SERVICIO == pS_SERVICIOS.COD_SERVICIO && d.COD_GRUPO == pS_SERVICIOS.COD_GRUPO && d.COD_SUBGRUPO == pS_SERVICIOS.COD_SUBGRUPO).Any())
                        {
                            db.PS_SERVICIOS.Add(pS_SERVICIOS);
                            db.SaveChanges();
                        }

                        var ClasificaServicio = new JavaScriptSerializer().Deserialize<List<PS_ClasificacionServicios>>(ClasificacionServicio);

                        if (ClasificaServicio != null && ClasificaServicio.Count > 0)
                        {
                            ClasificaServicio.ForEach(detalle =>
                            {
                                if (detalle.CLASIFICACION.ToUpper() == "POS CONDICIONADO")
                                {
                                    var ps_PosCondicionado = new List<PS_POS_CONDICIONADO>();

                                    var geLstPosCondicionado = (from pc in db.PS_POS_CONDICIONADO
                                                                join ds in db.PS_DETALLE_SERVICIOS on pc.ID equals ds.ID_PS_POS_CONDICIONADO into jDs
                                                                from ds in jDs
                                                                where ds.COD_SERVICIO == pS_SERVICIOS.COD_SERVICIO
                                                                    && ds.COD_GRUPO == pS_SERVICIOS.COD_GRUPO
                                                                    && ds.COD_SUBGRUPO == pS_SERVICIOS.COD_SUBGRUPO
                                                                    && ds.ID_PS_CLASIFICACION_POS == db.PS_CLASIFICACION_POS.FirstOrDefault(d => d.DESCRIPCION.Equals(detalle.CLASIFICACION)).ID
                                                                select pc);

                                    ///Si tiene diagnostico asignado
                                    if (!string.IsNullOrEmpty(detalle.DIAGNOSTICO))
                                    {
                                        detalle.DataDiagnostico?.ForEach(dg =>
                                        {
                                            {
                                                if (geLstPosCondicionado.FirstOrDefault(d => (d.DIAGNOSTICO == (dg.DIAGNOSTICO1))) == null)
                                                    ps_PosCondicionado.Add(new PS_POS_CONDICIONADO()
                                                    {
                                                        DIAGNOSTICO = dg.DIAGNOSTICO1,
                                                        CANTIDAD = detalle.DataCantidad != "" ? Convert.ToInt64(detalle.DataCantidad) : (long?)null,
                                                        MENSAJE_INFORMATIVO = MENSAJE_INFORMATIVO,
                                                    });
                                                else
                                                {
                                                    var gePosCondicionado = geLstPosCondicionado.FirstOrDefault(d => d.DIAGNOSTICO == dg.DIAGNOSTICO1);
                                                    gePosCondicionado.CANTIDAD = detalle.DataCantidad != "" ? Convert.ToInt64(detalle.DataCantidad) : (long?)null;
                                                    db.Entry(gePosCondicionado).State = System.Data.Entity.EntityState.Modified;
                                                    db.SaveChanges();
                                                }
                                            }
                                        });
                                    }
                                    ///Si no tiene diagnostico y si tiene grupo etareo asignado
                                    if (!string.IsNullOrEmpty(detalle.GRUPO_ETAREO))
                                    {
                                        detalle.DataGrupoEtareo?.ForEach(ge =>
                                        {
                                            if (geLstPosCondicionado.FirstOrDefault(d => d.ID_PS_GRUPOS_ETAREOS == ge.ID) == null)
                                                ps_PosCondicionado.Add(new PS_POS_CONDICIONADO()
                                                {
                                                    ID_PS_GRUPOS_ETAREOS = ge.ID,
                                                    CANTIDAD = detalle.DataCantidad != "" ? Convert.ToInt64(detalle.DataCantidad) : (long?)null,
                                                    MENSAJE_INFORMATIVO = MENSAJE_INFORMATIVO,
                                                });
                                        });
                                    }
                                    ///Si no tiene diagnostico y no tiene grupo etareo asignado y si tiene cantidad
                                    if (!string.IsNullOrEmpty(detalle.CANTIDAD))
                                    {
                                        ps_PosCondicionado.Add(new PS_POS_CONDICIONADO()
                                        {
                                            CANTIDAD = detalle.DataCantidad != "" ? Convert.ToInt64(detalle.DataCantidad) : (long?)null,
                                            MENSAJE_INFORMATIVO = MENSAJE_INFORMATIVO,
                                        });
                                    }

                                    ps_PosCondicionado.ForEach(psPosCond =>
                                    {
                                        db.PS_POS_CONDICIONADO.Add(psPosCond);
                                        db.SaveChanges();

                                        var psDetalleServicio = new PS_DETALLE_SERVICIOS()
                                        {
                                            COD_EPS = detalle.COD_EPS,
                                            COD_REGIONAL = detalle.COD_REGIONAL,
                                            COD_PLAN = detalle.COD_PLAN,
                                            COD_SERVICIO = pS_SERVICIOS.COD_SERVICIO,
                                            COD_GRUPO = pS_SERVICIOS.COD_GRUPO,
                                            COD_SUBGRUPO = pS_SERVICIOS.COD_SUBGRUPO,
                                            PS_POS_CONDICIONADO = psPosCond,
                                            COD_USUARIO = HttpContext.GetOwinContext().Authentication.User.Identity.Name,
                                            ID_PS_CLASIFICACION_POS = db.PS_CLASIFICACION_POS.FirstOrDefault(d => d.DESCRIPCION.TrimEnd() == detalle.CLASIFICACION).ID
                                        };

                                        db.PS_DETALLE_SERVICIOS.Add(psDetalleServicio);
                                        db.SaveChanges();
                                    });
                                }
                                else if (detalle.CLASIFICACION.ToUpper() == "POS")
                                {
                                    var psDetalleServicio = new PS_DETALLE_SERVICIOS()
                                    {
                                        COD_EPS = detalle.COD_EPS,
                                        COD_REGIONAL = detalle.COD_REGIONAL,
                                        COD_PLAN = detalle.COD_PLAN,
                                        COD_SERVICIO = pS_SERVICIOS.COD_SERVICIO,
                                        COD_GRUPO = pS_SERVICIOS.COD_GRUPO,
                                        COD_SUBGRUPO = pS_SERVICIOS.COD_SUBGRUPO,
                                        COD_USUARIO = HttpContext.GetOwinContext().Authentication.User.Identity.Name,
                                        //PS_POS_CONDICIONADO = new PS_POS_CONDICIONADO { MENSAJE_INFORMATIVO = MENSAJE_INFORMATIVO },
                                        ID_PS_CLASIFICACION_POS = db.PS_CLASIFICACION_POS.FirstOrDefault(d => d.DESCRIPCION.TrimEnd() == detalle.CLASIFICACION).ID
                                    };

                                    db.PS_DETALLE_SERVICIOS.Add(psDetalleServicio);
                                    db.SaveChanges();
                                }
                                else //NO POS
                                {
                                    var psDetalleServicio = new PS_DETALLE_SERVICIOS()
                                    {
                                        COD_EPS = detalle.COD_EPS,
                                        COD_REGIONAL = detalle.COD_REGIONAL,
                                        COD_PLAN = detalle.COD_PLAN,
                                        COD_SERVICIO = pS_SERVICIOS.COD_SERVICIO,
                                        COD_GRUPO = pS_SERVICIOS.COD_GRUPO,
                                        COD_SUBGRUPO = pS_SERVICIOS.COD_SUBGRUPO,
                                        COD_USUARIO = HttpContext.GetOwinContext().Authentication.User.Identity.Name,
                                        //PS_POS_CONDICIONADO = new PS_POS_CONDICIONADO { MENSAJE_INFORMATIVO = MENSAJE_INFORMATIVO },
                                        ID_PS_CLASIFICACION_POS = db.PS_CLASIFICACION_POS.FirstOrDefault(d => d.DESCRIPCION.TrimEnd() == "NO POS").ID
                                    };

                                    db.PS_DETALLE_SERVICIOS.Add(psDetalleServicio);
                                    db.SaveChanges();
                                }

                            });
                        }
                        else //NO POS
                        {
                            var psDetalleServicio = new PS_DETALLE_SERVICIOS()
                            {
                                COD_EPS = COD_EPS,
                                COD_REGIONAL = COD_REGIONAL,
                                COD_PLAN = COD_PLAN,
                                COD_SERVICIO = pS_SERVICIOS.COD_SERVICIO,
                                COD_GRUPO = pS_SERVICIOS.COD_GRUPO,
                                COD_SUBGRUPO = pS_SERVICIOS.COD_SUBGRUPO,
                                COD_USUARIO = HttpContext.GetOwinContext().Authentication.User.Identity.Name,
                                //PS_POS_CONDICIONADO = new PS_POS_CONDICIONADO { MENSAJE_INFORMATIVO = MENSAJE_INFORMATIVO },
                                ID_PS_CLASIFICACION_POS = db.PS_CLASIFICACION_POS.FirstOrDefault(d => d.DESCRIPCION.TrimEnd() == "NO POS").ID
                            };

                            db.PS_DETALLE_SERVICIOS.Add(psDetalleServicio);
                            db.SaveChanges();
                        }
                        //tran.Commit();

                        var paramView = "";
                        if (HttpContext.Request.UrlReferrer != null && HttpContext.Request.UrlReferrer.IsAbsoluteUri)
                        {
                            var nameValueCollection = HttpUtility.ParseQueryString(HttpContext.Request.UrlReferrer.Query);
                            if (nameValueCollection.Count > 0)
                            {
                                paramView = nameValueCollection["paramView"];
                            }
                        }

                        return RedirectToAction("Index", "Contratacion", new
                        {
                            paramView = paramView,
                            Editar = false
                        });

                    }

                    return RedirectToAction("Index", "Contratacion", new
                    {
                        paramView = HttpContext.Request.QueryString["paramView"],
                        Editar = false,
                    });
                }
                catch (Exception ex)
                {
                    //tran.Rollback();
                    UtilityEntities.sNameSpace = "GrupoEtareos.App.Areas.Contratacion.Controllers";
                    UtilityEntities.sClass = "ContratacionController.cs";
                    UtilityEntities.sOperation = "Create - POST";
                    UtilityEntities.exception = ex;
                    Utility.Utility.LogFile("Ocurrió un error al crear el registro, favor intente de nuevo en unos minutos");
                    return Json(new Exception("Ocurrió un error al crear el registro, favor intente de nuevo en unos minutos: Exception(): " + ex.Message), JsonRequestBehavior.DenyGet);
                }
            }
        }

        /// <summary>
        /// Se ejecuta cuando se envia el formulario con la opción de editar, por la opción POST.
        /// </summary>
        /// <param name="pS_SERVICIOS">Indica la entidad a crear con los parametros para la edición del servicio.</param>
        /// <param name="ClasificacionServicio">Objeto string que contiene un JSON enviado desde la vista para la clasificación parametrizada en el servicio</param>
        /// <param name="MENSAJE_INFORMATIVO">Indica el mensaje informativo enviado desde la creación servicio</param>
        /// <param name="COD_PLAN">Indica el código del plan seleccionado cuando el servicio es NO POS</param>
        /// <param name="COD_EPS">Indica el código de la eps seleccionada cuando el servicio es NO POS</param>
        /// <param name="COD_REGIONAL">Indica el código de la regional seleccionada cuando el servicio es NO POS</param>
        /// <returns>Retorna la vista a ejecutar al momento de realizar la edición del servicio</returns>
        // POST: Contratacion/Contratacion/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = FieldEntitie)] PS_SERVICIOS pS_SERVICIOS, string ClasificacionServicio, string MENSAJE_INFORMATIVO, string COD_PLAN, string COD_EPS, string COD_REGIONAL)
        {
            //using (var tran = db.Database.BeginTransaction())
            {
                try
                {
                    ModelState["AUTORIZACION_INICIAL"].Errors.Clear();
                    ModelState["VALOR_VARIABLE"].Errors.Clear();
                    var strCodigoEPS = "";
                    var strCodigoRegional = "";

                    if (ModelState.IsValid)
                    {
                        pS_SERVICIOS.FECHA_CREACION = DateTime.Now;
                        pS_SERVICIOS.AUTORIZACION_INICIAL = "S";
                        pS_SERVICIOS.VALOR_VARIABLE = "S";
                        //pS_SERVICIOS.APLICA_COPAGO = pS_SERVICIOS.APLICA_COPAGO == null ? false : pS_SERVICIOS.APLICA_COPAGO;
                        //pS_SERVICIOS.APLICA_CUOTAMODERADORA = pS_SERVICIOS.APLICA_CUOTAMODERADORA == null ? false : pS_SERVICIOS.APLICA_CUOTAMODERADORA;
                        if (db.PS_SERVICIOS.Where(d => d.COD_SERVICIO == pS_SERVICIOS.COD_SERVICIO && d.COD_GRUPO == pS_SERVICIOS.COD_GRUPO && d.COD_SUBGRUPO == pS_SERVICIOS.COD_SUBGRUPO).Any())
                        {
                            db.Entry(pS_SERVICIOS).State = System.Data.Entity.EntityState.Modified;
                            db.SaveChanges();
                        }

                        var ClasificaServicio = new JavaScriptSerializer().Deserialize<List<PS_ClasificacionServicios>>(ClasificacionServicio);

                        if (ClasificaServicio != null & ClasificaServicio.Count > 0)
                        {
                            ClasificaServicio.ForEach(detalle =>
                            {
                                strCodigoEPS = detalle.COD_EPS;
                                strCodigoRegional = detalle.COD_REGIONAL;

                                if (detalle.CLASIFICACION.ToUpper() == "POS CONDICIONADO")
                                {
                                    var ps_PosCondicionado = new List<PS_POS_CONDICIONADO>();

                                    var geLstPosCondicionado = (from pc in db.PS_POS_CONDICIONADO
                                                                join ds in db.PS_DETALLE_SERVICIOS on pc.ID equals ds.ID_PS_POS_CONDICIONADO into jDs
                                                                from ds in jDs
                                                                where ds.COD_SERVICIO == pS_SERVICIOS.COD_SERVICIO
                                                                   && ds.COD_GRUPO == pS_SERVICIOS.COD_GRUPO
                                                                   && ds.COD_SUBGRUPO == pS_SERVICIOS.COD_SUBGRUPO
                                                                   && ds.ID_PS_CLASIFICACION_POS == db.PS_CLASIFICACION_POS.FirstOrDefault(d => d.DESCRIPCION.Equals(detalle.CLASIFICACION)).ID
                                                                select pc);

                                    ///Si tiene diagnostico asignado
                                    if (!string.IsNullOrEmpty(detalle.DIAGNOSTICO))
                                    {
                                        detalle.DataDiagnostico?.ForEach(dg =>
                                        {
                                            if (geLstPosCondicionado.FirstOrDefault(d => (d.DIAGNOSTICO == (dg.DIAGNOSTICO1))) == null)
                                                ps_PosCondicionado.Add(new PS_POS_CONDICIONADO()
                                                {
                                                    DIAGNOSTICO = dg.DIAGNOSTICO1,
                                                    CANTIDAD = detalle.DataCantidad != "" ? Convert.ToInt64(detalle.DataCantidad) : (long?)null,
                                                    MENSAJE_INFORMATIVO = MENSAJE_INFORMATIVO,
                                                });
                                        });
                                    }
                                    ///Si no tiene diagnostico y si tiene grupo etareo asignado
                                    if (!string.IsNullOrEmpty(detalle.GRUPO_ETAREO))
                                    {
                                        detalle.DataGrupoEtareo?.ForEach(ge =>
                                        {
                                            if (geLstPosCondicionado.FirstOrDefault(d => d.ID_PS_GRUPOS_ETAREOS == ge.ID) == null)
                                                ps_PosCondicionado.Add(new PS_POS_CONDICIONADO()
                                                {
                                                    ID_PS_GRUPOS_ETAREOS = ge.ID,
                                                    CANTIDAD = detalle.DataCantidad != "" ? Convert.ToInt64(detalle.DataCantidad) : (long?)null,
                                                    MENSAJE_INFORMATIVO = MENSAJE_INFORMATIVO,
                                                });
                                        });
                                    }
                                    ///Si no tiene diagnostico y no tiene grupo etareo asignado y si tiene cantidad
                                    if (!string.IsNullOrEmpty(detalle.CANTIDAD))
                                    {
                                        ps_PosCondicionado.Add(new PS_POS_CONDICIONADO()
                                        {
                                            CANTIDAD = detalle.DataCantidad != "" ? Convert.ToInt64(detalle.DataCantidad) : (long?)null,
                                            MENSAJE_INFORMATIVO = MENSAJE_INFORMATIVO,
                                        });
                                    }

                                    ps_PosCondicionado.ForEach(psPosCond =>
                                    {
                                        db.PS_POS_CONDICIONADO.Add(psPosCond);
                                        db.SaveChanges();

                                        var psDetalleServicioPos = db.PS_DETALLE_SERVICIOS.FirstOrDefault(d => d.COD_SERVICIO == pS_SERVICIOS.COD_SERVICIO
                                                                                                            && d.COD_SUBGRUPO == pS_SERVICIOS.COD_SUBGRUPO
                                                                                                            && d.COD_GRUPO == pS_SERVICIOS.COD_GRUPO
                                                                                                            && d.ID_PS_CLASIFICACION_POS == db.PS_CLASIFICACION_POS.FirstOrDefault(cla => cla.DESCRIPCION.TrimEnd() == "POS").ID);

                                        if (psDetalleServicioPos != null)
                                        {
                                            psDetalleServicioPos.PS_POS_CONDICIONADO = psPosCond;
                                            psDetalleServicioPos.ID_PS_CLASIFICACION_POS = db.PS_CLASIFICACION_POS.FirstOrDefault(d => d.DESCRIPCION.TrimEnd() == detalle.CLASIFICACION).ID;
                                            db.Entry(psDetalleServicioPos).State = System.Data.Entity.EntityState.Modified;
                                        }
                                        else
                                        {
                                            var psDetalleServicio = new PS_DETALLE_SERVICIOS()
                                            {
                                                COD_EPS = detalle.COD_EPS,
                                                COD_REGIONAL = detalle.COD_REGIONAL,
                                                COD_PLAN = detalle.COD_PLAN,
                                                COD_SERVICIO = pS_SERVICIOS.COD_SERVICIO,
                                                COD_GRUPO = pS_SERVICIOS.COD_GRUPO,
                                                COD_SUBGRUPO = pS_SERVICIOS.COD_SUBGRUPO,
                                                PS_POS_CONDICIONADO = psPosCond,
                                                COD_USUARIO = HttpContext.GetOwinContext().Authentication.User.Identity.Name,
                                                ID_PS_CLASIFICACION_POS = db.PS_CLASIFICACION_POS.FirstOrDefault(d => d.DESCRIPCION.TrimEnd() == detalle.CLASIFICACION).ID
                                            };

                                            db.PS_DETALLE_SERVICIOS.Add(psDetalleServicio);
                                        }
                                        db.SaveChanges();
                                    });
                                }
                                else if (detalle.CLASIFICACION.ToUpper() == "POS")
                                {
                                    var psDetalleServicio = db.PS_DETALLE_SERVICIOS.Where(d => d.COD_SERVICIO == pS_SERVICIOS.COD_SERVICIO && d.COD_SUBGRUPO == pS_SERVICIOS.COD_SUBGRUPO && d.COD_GRUPO == pS_SERVICIOS.COD_GRUPO);
                                    foreach (var item in psDetalleServicio)
                                    {
                                        item.COD_EPS = detalle.COD_EPS;
                                        item.COD_REGIONAL = detalle.COD_REGIONAL;
                                        item.COD_PLAN = detalle.COD_PLAN;
                                        //item.PS_POS_CONDICIONADO.MENSAJE_INFORMATIVO = MENSAJE_INFORMATIVO;
                                        item.COD_USUARIO = HttpContext.GetOwinContext().Authentication.User.Identity.Name;
                                        item.ID_PS_CLASIFICACION_POS = db.PS_CLASIFICACION_POS.FirstOrDefault(d => d.DESCRIPCION.TrimEnd() == detalle.CLASIFICACION).ID;

                                        db.Entry(item).State = System.Data.Entity.EntityState.Modified;
                                    }
                                    db.SaveChanges();
                                }
                                else //NO POS
                                {
                                    var psDetalleServicio = db.PS_DETALLE_SERVICIOS.Where(d => d.COD_SERVICIO == pS_SERVICIOS.COD_SERVICIO && d.COD_SUBGRUPO == pS_SERVICIOS.COD_SUBGRUPO && d.COD_GRUPO == pS_SERVICIOS.COD_GRUPO);
                                    foreach (var item in psDetalleServicio)
                                    {
                                        item.COD_EPS = detalle.COD_EPS;
                                        item.COD_REGIONAL = detalle.COD_REGIONAL;
                                        item.COD_PLAN = detalle.COD_PLAN;
                                        //item.PS_POS_CONDICIONADO.MENSAJE_INFORMATIVO = MENSAJE_INFORMATIVO;
                                        item.COD_USUARIO = HttpContext.GetOwinContext().Authentication.User.Identity.Name;
                                        item.ID_PS_CLASIFICACION_POS = db.PS_CLASIFICACION_POS.FirstOrDefault(d => d.DESCRIPCION.TrimEnd() == "NO POS").ID;

                                        db.Entry(item).State = System.Data.Entity.EntityState.Modified;
                                    }
                                    db.SaveChanges();
                                }
                            });
                        }
                        else //NO POS
                        {
                            var psDetalleServicio = db.PS_DETALLE_SERVICIOS.Where(d => d.COD_SERVICIO == pS_SERVICIOS.COD_SERVICIO && d.COD_SUBGRUPO == pS_SERVICIOS.COD_SUBGRUPO && d.COD_GRUPO == pS_SERVICIOS.COD_GRUPO);
                            foreach (var item in psDetalleServicio)
                            {
                                item.COD_EPS = COD_EPS;
                                item.COD_REGIONAL = COD_REGIONAL;
                                item.COD_PLAN = COD_PLAN;
                                //item.PS_POS_CONDICIONADO.MENSAJE_INFORMATIVO = MENSAJE_INFORMATIVO;
                                item.COD_USUARIO = HttpContext.GetOwinContext().Authentication.User.Identity.Name;
                                item.ID_PS_CLASIFICACION_POS = db.PS_CLASIFICACION_POS.FirstOrDefault(d => d.DESCRIPCION.TrimEnd() == "NO POS").ID;

                                db.Entry(item).State = System.Data.Entity.EntityState.Modified;
                            }
                            db.SaveChanges();
                        }

                        //tran.Commit();

                        var paramView = "";
                        if (HttpContext.Request.UrlReferrer != null && HttpContext.Request.UrlReferrer.IsAbsoluteUri)
                        {
                            var nameValueCollection = HttpUtility.ParseQueryString(HttpContext.Request.UrlReferrer.Query);
                            if (nameValueCollection.Count > 0)
                            {
                                paramView = nameValueCollection["paramView"];
                            }
                        }

                        return RedirectToAction("Index", "Contratacion", new
                        {
                            paramView = paramView,
                            Editar = true
                        });
                    }

                    return RedirectToAction("Index", "Contratacion", new
                    {
                        paramView = HttpContext.Request.QueryString["paramView"],
                        Editar = true,
                    });
                }
                catch (Exception ex)
                {
                    //tran.Rollback();
                    UtilityEntities.sNameSpace = "GrupoEtareos.App.Areas.Contratacion.Controllers";
                    UtilityEntities.sClass = "ContratacionController.cs";
                    UtilityEntities.sOperation = "Edit - POST";
                    UtilityEntities.exception = ex;
                    Utility.Utility.LogFile("Ocurrió un error al editar el servicio, favor intente de nuevo en unos minutos");
                    return Json(new Exception("Ocurrió un error al editar el servicio, favor intente de nuevo en unos minutos: Exception(): " + ex.Message), JsonRequestBehavior.DenyGet);
                }
            };
        }

        /// <summary>
        /// Realiza la eliminación de la opción eliminar de la clasificación de servicio para Diagnostico y Grupo Etareo
        /// </summary>
        /// <param name="codServicio">Indica el codigo de servicio a consultar para la eliminación de la clasificación seleccionada</param>
        /// <param name="codGrupo">Indica el codigo del grupo o indicador de servicio a consultar para la eliminación de la clasificación seleccionada</param>
        /// <param name="codSubGrupo">Indica el codigo del subgrupo a consultar para la eliminación de la clasificación seleccionada</param>
        /// <param name="ID_PS_POS_CONDICIONADO">Indentificador del Id si es POS CONDICIONADO a consultar para la eliminación de la clasificación seleccionada</param>
        /// <param name="funcCall">Inidica el llamado de la función que lo solicita "Diagnostico" o "Grupo Etareo"</param>
        /// <returns>Retorna un JSON con la respuesta a la ejecución de la función</returns>
        public ActionResult DeleteClasificaServicio(string codServicio, string codGrupo, string codSubGrupo, long? ID_PS_POS_CONDICIONADO, string funcCall)
        {
            try
            {
                var dsDiagnostico = db.PS_DETALLE_SERVICIOS.FirstOrDefault(d => d.COD_SERVICIO == codServicio && d.COD_GRUPO == codGrupo && d.COD_SUBGRUPO == codSubGrupo && d.ID_PS_POS_CONDICIONADO == ID_PS_POS_CONDICIONADO);

                db.PS_DETALLE_SERVICIOS.Remove(dsDiagnostico);
                db.SaveChanges();

                var psPosCondi = db.PS_POS_CONDICIONADO.First(d => d.ID == ID_PS_POS_CONDICIONADO);
                db.PS_POS_CONDICIONADO.Remove(psPosCondi);
                db.SaveChanges();
            }
            catch (Exception ex)
            {
                UtilityEntities.sNameSpace = "GrupoEtareos.App.Areas.Contratacion.Controllers";
                UtilityEntities.sClass = "ContratacionController.cs";
                UtilityEntities.sOperation = "DeleteDiagnostico";
                UtilityEntities.exception = ex;
                Utility.Utility.LogFile("Ocurrió un error al eliminar un " + funcCall + " de la clasificación del servicio, favor intente de nuevo en unos minutos");
                return Json(new JsonResult { Data = new { Message = "Ocurrió un error al eliminar un " + funcCall + " de la clasificación del servicio, favor intente de nuevo en unos minutos: Exception(): " + ex.Message, Ok = false } }, JsonRequestBehavior.AllowGet);
            }

            return Json(new JsonResult { Data = new { Message = funcCall + " eliminado satisfactoriamente!", Ok = true } }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Carga las listas o combos del formulario para la sección de DATOS DEL SERVICO >>
        /// </summary>
        public void CargarListasFormulario(PS_SERVICIOS pS_SERVICIOS, bool Editar)
        {
            ///Cargar Combo de Indicador de Servicio
            ViewBag.IndicadorServicio = new SelectList(db.GRUPOes
                                                        .Where(d => d.MANUAL_TARIFARIO == "N")
                                                        .OrderBy(d => d.DESCR_GRUPO.TrimEnd() + " | " + d.COD_GRUPO)
                                                        .Select(d => new
                                                        {
                                                            COD_GRUPO = d.COD_GRUPO, //.TrimEnd(),
                                                            DESCRIPCION = d.DESCR_GRUPO.TrimEnd() + " | " + d.COD_GRUPO,
                                                            ABRE_GRUPO = d.ABRE_GRUPO
                                                        }), "COD_GRUPO", "DESCRIPCION", (Editar && pS_SERVICIOS != null ? pS_SERVICIOS.COD_GRUPO : null));

            ///Cargar Combo De SubGrupo
            ViewBag.SubGrupo = new SelectList((Editar && pS_SERVICIOS != null ? ListaSubGrupo(4, pS_SERVICIOS.COD_GRUPO) : ListaSubGrupo(1, null)), "cod_subgrupo", "descr_subgrupo", (Editar && pS_SERVICIOS != null ? pS_SERVICIOS.COD_SUBGRUPO : null));

            ///Cargar Combo de Clasificación Contable
            ViewBag.Gn_ClasificaContable = new SelectList(CargarListaClasificacionContable(), "COD_CLASIF_CONTABLE", "DESC_CONTABLE", (Editar && pS_SERVICIOS != null ? pS_SERVICIOS.COD_CLASIF_CONTABLE : null));
            ///Cargar Combo de Aplica Especialidad
            ViewBag.AplicaEspecialidad = new SelectList(CargarListasSiNo(), "Value", "Text");
            ///Cargar Combo de CContable Evento
            ViewBag.Gn_ConceptoContableEvento = new SelectList(CargarListaConceptoContableCapitacion("E"), "CONCEPTO_CONTABLE", "DESCRIPCION", (Editar && pS_SERVICIOS != null ? pS_SERVICIOS.COD_CONCEPTOC_EVENTO : (decimal?)null));
            ///Cargar Combo CContable Capita
            ViewBag.Gn_ConceptoCapitacion = new SelectList(CargarListaConceptoContableCapitacion("C"), "CONCEPTO_CONTABLE", "DESCRIPCION", (Editar && pS_SERVICIOS != null ? pS_SERVICIOS.COD_CONCEPTOC_CAPITA : (decimal?)null));
            ///Cargar Combo de Complejidad
            ViewBag.Complejidad = new SelectList(CargarListaComplejidad(), "COD_NIVEL_COMPL", "DESC_NIVEL_COMPL", (Editar && pS_SERVICIOS != null ? pS_SERVICIOS.COMPLEJIDAD : null));
            ///Cargar Combo de TIpo de Servicio
            ViewBag.TipoServicio = new SelectList(CargarListaTipoServicios(), "COD_TIPO_SERVICIO", "DESCRIPCION", (Editar && pS_SERVICIOS != null ? pS_SERVICIOS.COD_TIPO_SERVICIO : null));
            ///Cargar Combo de Regimen
            ViewBag.Regimen = new SelectList(CargarRegimen(), "COD_PLAN", "DESC_PLAN");
            ///Cargar Combo de Clasificación Pos
            ViewBag.ClasificacionPos = new SelectList(CargarClasificacionPos(), "ID", "DESCRIPCION");
            ///Cargar Combo de Grupos Etareos
            ViewBag.GruposEtareos = new SelectList(CargarGruposEtareos(), "ID", "DESC_GRUPOETAREO");
            ///Cargar Combo de Sexo
            ViewBag.Sexo = new SelectList(CargarSexo(), "COD_SEXO", "DESC_SEXO");
            ///Carga todos los Combos con la opción de "SI" y "NO"
            ViewBag.ListasSiNo = new SelectList(CargarListasSiNo(), "Value", "Text");
            ///Cargar Combo de Profesional Autoriza
            ViewBag.ProfesionalAutoriza = new SelectList(CargarProfesionalAutoriza(), "PROFESIONAL_AUTORIZA", "DESC_PROFESIONAL_AUTORIZA");
            ///Cargar Combo de Nivel Autorización
            ViewBag.NivelAutorizacion = new SelectList(CargarListaNivelAutorizacion(), "NIVEL_AUTORIZACION", "DESC_AUTORIZACION");

            ViewBag.TecnologiaDCI = new SelectList(ConsultarDCI("", "", 0), "ID", "DESCRIPCION_SERVICIO", (Editar && pS_SERVICIOS != null ? pS_SERVICIOS.ID_TECNOLOGIA : null));
            ///Cargar Combo de Nivel Autorización
            ViewBag.Estado = new SelectList(LLenarEstados(), "ID", "DECRIPCION", (Editar && pS_SERVICIOS != null ? pS_SERVICIOS.ID_ESTADO_SERVICIOS.ToString() : "1"));

            if (Editar && pS_SERVICIOS != null)
            {
                ViewBag.dsClasifica = CargarListaClasificacionServicio(pS_SERVICIOS).ToList(); //SI TIENE UNA CLASIFICACIÓN CONFIGURADA EL SERVICIO
            }
        }

        /// <summary>
        /// Ejecuta la carga de los subgrupos por filtro seleccionado
        /// </summary>
        /// <param name="tipoConsulta">Indica el tipo de consulta a realizar para la carga de subgrupos</param>
        /// <param name="codGrupo">Indica el código del grupo o indicador de servicio por el cual se va a consultar el Subgrupo</param>
        /// <param name="codSubGrupo">Indica el código del subgrupo por el cual se va a realizar la consulta, este campo es para el modal de busqueda de subgrupo</param>
        /// <param name="descr_subgrupo">Indica la descripción del subgrupo por el cual se va a realizar la consulta, este campo es para el modal de busqueda de subgrupo</param>
        /// <returns>Retorna un objeto JSON que es consumido desde un ajax ejecutado por la vista</returns>
        public ActionResult CargarListaSubGrupo(int tipoConsulta, string codGrupo, string codSubGrupo = "", string descr_subgrupo = "")
        {
            return Json(ListaSubGrupo(tipoConsulta, codGrupo, codSubGrupo, descr_subgrupo), JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Funcion que se ejecuta para la carga de la información de la clasificación del servicio
        /// </summary>
        /// <param name="pS_SERVICIOS">Entidad a consultar que contiene los campos de la entidad servicio a ser editada.</param>
        /// <returns>Retorna un objeto IEnumerable<object> con la lista de la clasificación del servicio</returns>
        public IEnumerable<object> CargarListaClasificacionServicio(PS_SERVICIOS pS_SERVICIOS)
        {
            try
            {
                var dsClasifica = from cpos in db.PS_CLASIFICACION_POS.AsEnumerable()
                                  join ds in pS_SERVICIOS.PS_DETALLE_SERVICIOS.AsEnumerable() on cpos.ID equals ds.ID_PS_CLASIFICACION_POS into jDs
                                  from ds in jDs
                                  where ds.COD_SERVICIO == pS_SERVICIOS.COD_SERVICIO
                                    && ds.COD_GRUPO == pS_SERVICIOS.COD_GRUPO
                                    && ds.COD_SUBGRUPO == pS_SERVICIOS.COD_SUBGRUPO
                                  group new
                                  {
                                      ds.ID_PS_POS_CONDICIONADO
                                  } by new
                                  {
                                      cpos.DESCRIPCION,
                                      ds.COD_EPS,
                                      ds.COD_PLAN,
                                      ds.COD_REGIONAL,
                                      ds.GN_PLANES.DESC_PLAN
                                  } into g
                                  select new
                                  {
                                      CLASIFICACION = g.Key.DESCRIPCION,
                                      COD_EPS = g.Key.COD_EPS,
                                      COD_PLAN = g.Key.COD_PLAN,
                                      COD_REGIONAL = g.Key.COD_REGIONAL,
                                      Regimen = g.Key.DESC_PLAN,
                                      DIAGNOSTICO = "",
                                      GRUPO_ETAREO = "",
                                      CANTIDAD = "",
                                      Condicion = db.PS_CONDICIONES.ToList(),
                                      DataDiagnostico = (from pc in db.PS_POS_CONDICIONADO.AsEnumerable()
                                                         join ds in g on pc.ID equals ds.ID_PS_POS_CONDICIONADO into jDs
                                                         from ds in jDs
                                                         where pc.DIAGNOSTICO != null
                                                         select new
                                                         {
                                                             DIAGNOSTICO1 = pc.DIAGNOSTICO,
                                                             DESCR_DIAGNOSTICO = pc.DIAGNOSTICO1.DESCR_DIAGNOSTICO,
                                                             ID_PS_POS_CONDICIONADO = pc.ID
                                                         }),
                                      DataGrupoEtareo = (from pc in db.PS_POS_CONDICIONADO.AsEnumerable()
                                                         join ds in g on pc.ID equals ds.ID_PS_POS_CONDICIONADO into jDs
                                                         from ds in jDs
                                                         where pc.ID_PS_GRUPOS_ETAREOS != null
                                                         select new
                                                         {
                                                             ID = pc.ID_PS_GRUPOS_ETAREOS,
                                                             DESC_GRUPOETAREO = pc.PS_GRUPOS_ETAREOS.DESC_GRUPOETAREO,
                                                             ID_PS_POS_CONDICIONADO = pc.ID
                                                         }),
                                      DataCantidad = (from pc in db.PS_POS_CONDICIONADO.AsEnumerable()
                                                      join ds in g on pc.ID equals ds.ID_PS_POS_CONDICIONADO into jDs
                                                      from ds in jDs
                                                      where pc.CANTIDAD != null
                                                      select new
                                                      {
                                                          CANTIDAD = pc.CANTIDAD
                                                      }).FirstOrDefault()?.CANTIDAD
                                  };

                return dsClasifica;
            }
            catch (Exception ex)
            {
                UtilityEntities.sNameSpace = "GrupoEtareos.App.Areas.Contratacion.Controllers";
                UtilityEntities.sClass = "ContratacionController.cs";
                UtilityEntities.sOperation = "CargarListaClasificacionServicio";
                UtilityEntities.exception = ex;
                Utility.Utility.LogFile("Ocurrió un error al consultar la clasificiación del servicio, favor intente de nuevo en unos minutos");
                //return Json(new Exception("Ocurrió un error al consultar los sub grupos, favor intente de nuevo en unos minutos: Exception(): " + ex.Message), JsonRequestBehavior.DenyGet);
                throw new Exception("Ocurrió un error al consultar la clasificiación del servicio, favor intente de nuevo en unos minutos: Exception(): " + ex.Message);
            }
        }

        /// <summary>
        /// Carga la lista de los subgrupos por filtro seleccionado en el campo Indicador Servicio
        /// </summary>
        /// <param name="tipoConsulta">Indica el tipo de consulta a realizar</param>
        /// <param name="codGrupo">Indica el códgio del grupo del campo Indicador Servicio</param>
        /// <param name="codSubGrupo">Indica el Código del SubGrupo a consultar</param>
        /// <param name="descr_subgrupo">Indica la Descripción SubGrupo a consultar</param>
        /// <returns></returns>
        public IEnumerable<object> ListaSubGrupo(int tipoConsulta, string codGrupo, string codSubGrupo = "", string descr_subgrupo = "")
        {
            IEnumerable<object> objDatos = null;

            switch (tipoConsulta)
            {
                case 1: //Consulta Inicial
                    objDatos = new List<SUB_GRUPO>() { new SUB_GRUPO { } };
                    break;
                case 2: //Consulta por Codigo SubGrupo
                    try
                    {
                        objDatos = db.P_CONSULTAR_SUB_GRUPO_X_FILTRO(codGrupo, codSubGrupo, "").Select(d => new SUB_GRUPO { COD_GRUPO = d.cod_grupo, COD_SUBGRUPO = d.cod_subgrupo, DESCR_SUBGRUPO = d.descr_subgrupo }).ToList();
                    }
                    catch (Exception ex)
                    {
                        UtilityEntities.sNameSpace = "GrupoEtareos.App.Areas.Contratacion.Controllers";
                        UtilityEntities.sClass = "ContratacionController.cs";
                        UtilityEntities.sOperation = "CargarListaSubGrupo";
                        UtilityEntities.exception = ex;
                        Utility.Utility.LogFile("Ocurrió un error al consultar los sub grupos, favor intente de nuevo en unos minutos");
                        throw new Exception("Ocurrió un error al consultar los sub grupos, favor intente de nuevo en unos minutos: Exception(): " + ex.Message);
                    }
                    break;
                case 3: //Consulta por Descripción SubGrupo
                    try
                    {
                        objDatos = db.P_CONSULTAR_SUB_GRUPO_X_FILTRO(codGrupo, "", descr_subgrupo).Select(d => new SUB_GRUPO { COD_GRUPO = d.cod_grupo, COD_SUBGRUPO = d.cod_subgrupo, DESCR_SUBGRUPO = d.descr_subgrupo }).ToList();
                    }
                    catch (Exception ex)
                    {
                        UtilityEntities.sNameSpace = "GrupoEtareos.App.Areas.Contratacion.Controllers";
                        UtilityEntities.sClass = "ContratacionController.cs";
                        UtilityEntities.sOperation = "CargarListaSubGrupo";
                        UtilityEntities.exception = ex;
                        Utility.Utility.LogFile("Ocurrió un error al consultar los sub grupos, favor intente de nuevo en unos minutos");
                        throw new Exception("Ocurrió un error al consultar los sub grupos, favor intente de nuevo en unos minutos: Exception(): " + ex.Message);
                    }
                    break;
                case 4: //Consulta por Grupo
                    try
                    {
                        objDatos = db.P_CONSULTAR_SUB_GRUPO_X_FILTRO(codGrupo, "", "").Select(d => new SUB_GRUPO { COD_GRUPO = d.cod_grupo, COD_SUBGRUPO = d.cod_subgrupo, DESCR_SUBGRUPO = d.descr_subgrupo }).ToList();
                    }
                    catch (Exception ex)
                    {
                        UtilityEntities.sNameSpace = "GrupoEtareos.App.Areas.Contratacion.Controllers";
                        UtilityEntities.sClass = "ContratacionController.cs";
                        UtilityEntities.sOperation = "CargarListaSubGrupo";
                        UtilityEntities.exception = ex;
                        Utility.Utility.LogFile("Ocurrió un error al consultar los sub grupos, favor intente de nuevo en unos minutos");
                        throw new Exception("Ocurrió un error al consultar los sub grupos, favor intente de nuevo en unos minutos: Exception(): " + ex.Message);
                    }
                    break;
            }

            return objDatos;
            //return Json(objDatos, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Carga la lista de las clasificaciones contables
        /// </summary>
        /// <returns>Retorna un objeto IEnumerable<object> con la lista de la clasificación de los conceptos contables</returns>
        public IEnumerable<object> CargarListaClasificacionContable()
        {
            var objDatos = from gn_CC in db.GN_CLASIFIC_CONTABLE
                           orderby gn_CC.DESC_CONTABLE
                           select new
                           {
                               COD_CLASIF_CONTABLE = gn_CC.COD_CLASIF_CONTABLE,
                               DESC_CONTABLE = gn_CC.DESC_CONTABLE
                           };
            return objDatos;
        }

        /// <summary>
        /// Funcion que se ejecuta para la carga los conceptos contables
        /// </summary>
        /// <param name="tipoConsulta">Indica el tipo de consulta a realizar</param>
        /// <param name="tipoContrato">Indica el tipo de contrato por el cual se realiza la consulta</param>
        /// <param name="cod_centro_costo">Indica el código del centro de costo por el cual se realiza la consulta, este campo se usa para la opción de consulta para centro de costos</param>
        /// <param name="desc_centro_costo">Indica la descripción del centro de costo por el cual se realiza la consulta, este campo se usa para la opción de consulta para centro de costos</param>
        /// <returns>Retorna un objeto JSON que es consumido desde un ajax ejecutado por la vista</returns>
        public ActionResult CargarLstConceptoContableEvento(long tipoConsulta = 0, string tipoContrato = "E", long cod_centro_costo = 0, string desc_centro_costo = "")
        {
            var objDatos = from cc in db.GN_CONCEPTOS_CONTABLES
                           where cc.ESTADO_CONCEPTO == "1"
                            && cc.TIPO_CONTRATO == tipoContrato
                            || (string.IsNullOrEmpty(desc_centro_costo) ? cc.CONCEPTO_CONTABLE == cod_centro_costo : cc.DESC_CONCEPTO_CONTABLE.TrimEnd().Contains(desc_centro_costo))
                           select new
                           {
                               CONCEPTO_CONTABLE = cc.CONCEPTO_CONTABLE,
                               DESCRIPCION = cc.CONCEPTO_CONTABLE + " | " + cc.DESC_CONCEPTO_CONTABLE.TrimEnd(),
                               TIPO_CONTRATO = cc.TIPO_CONTRATO
                           };
            return Json(objDatos, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Carga la lista de las clasificaciones contables por capita o evento
        /// </summary>
        /// <param name="tipoContrato">Parametro de tipo string que Indica el tipo de contrato por el cual se realiza la consulta por defecto "C"</param>
        /// <param name="intConceptoContable">Parametro de tipo entero que Indica el concepto contable a consultar por defecto es "0"</param>
        /// <returns>Retorna un objeto IEnumerable<object> con la lista de la clasificación de los conceptos contables por capita o evento</returns>
        public IEnumerable<object> CargarListaConceptoContableCapitacion(string tipoContrato = "C", int intConceptoContable = 0)
        {
            var objDatos = from cc in db.GN_CONCEPTOS_CONTABLES
                           where cc.ESTADO_CONCEPTO == "1"
                            && cc.TIPO_CONTRATO == tipoContrato
                            || cc.CONCEPTO_CONTABLE == intConceptoContable
                           select new
                           {
                               CONCEPTO_CONTABLE = cc.CONCEPTO_CONTABLE,
                               DESCRIPCION = cc.CONCEPTO_CONTABLE + " | " + cc.DESC_CONCEPTO_CONTABLE.TrimEnd(),
                               TIPO_CONTRATO = cc.TIPO_CONTRATO
                           };
            return objDatos;
        }

        /// <summary>
        /// Carga la lista de la complejidad
        /// </summary>
        /// <returns>Retorna un objeto IEnumerable<object> con la lista de la complejidad</returns>
        public IEnumerable<object> CargarListaComplejidad()
        {
            var objDatos = from cc in db.NIVEL_COMPLEJIDAD
                           orderby cc.DESC_NIVEL_COMPL
                           select new
                           {
                               COD_NIVEL_COMPL = cc.COD_NIVEL_COMPL,
                               DESC_NIVEL_COMPL = cc.DESC_NIVEL_COMPL,
                               COD_GRADO_COMPLEJIDAD = cc.COD_GRADO_COMPLEJIDAD
                           };
            return objDatos;
        }

        /// <summary>
        /// Carga la lista de los tipos de servicios
        /// </summary>
        /// <param name="blFiltro">Indica el tipo de filtro a realizar. Ej. (blFiltro ? cc.COD_TIPO_SERVICIO != 1 : true)</param>
        /// <returns>Retorna un objeto IEnumerable<object> con la lista de los tipos de servicios</returns>
        public IEnumerable<object> CargarListaTipoServicios(bool blFiltro = false)
        {
            var objDatos = from cc in db.GN_TIPO_SERVICIOS
                           where (blFiltro ? cc.COD_TIPO_SERVICIO != 1 : true)
                           select new
                           {
                               COD_TIPO_SERVICIO = cc.COD_TIPO_SERVICIO,
                               DESCRIPCION = cc.DESCRIPCION
                           };
            return objDatos;
        }

        /// <summary>
        /// Carga la lista de la opción Tecnología
        /// </summary>
        /// <param name="strCodDCI">Código de la tecnología a buscar</param>
        /// <param name="strDescDCI">Descripión de la tecnología a buscar</param>
        /// <param name="intTipoServicio">Tipo de servicio que se esta seleccionado</param>
        /// <returns>Retorna un objeto IEnumerable<object> con la lista de la Tecnología</returns>
        public IEnumerable<object> ConsultarDCI(string strCodDCI, string strDescDCI, int intTipoServicio = 0)
        {
            var objDatos = db.P_PS_CONSULTAR_TIPO_TECNOLOGIA(strCodDCI, strDescDCI, intTipoServicio).ToList();
            return objDatos;
        }

        /// <summary>
        /// Carga la lista de la opción Tecnología
        /// </summary>
        /// <param name="strCodDCI">Código de la tecnología a buscar</param>
        /// <param name="strDescDCI">Descripión de la tecnología a buscar</param>
        /// <param name="intTipoServicio">Tipo de servicio que se esta seleccionado</param>
        /// <returns>Retorna un objeto JSON que es consumido desde un ajax ejecutado por la vista para el modal de busqueda de la tecnología</returns>
        public ActionResult ConsultarModalTecnologiaDCI(string strCodDCI, string strDescDCI, int intTipoServicio = 0)
        {
            return Json(ConsultarDCI(strCodDCI, strDescDCI, intTipoServicio), JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Carga la lista de los estados para la creación o edición del servicio
        /// </summary>
        /// <returns>Retorna un objeto IEnumerable<object> con la lista de los estados del servicio</returns>
        public IEnumerable<object> LLenarEstados()
        {
            var objDatos = from cc in db.SV_ESTADO_SERVICIOS
                           select new
                           {
                               ID = cc.ID,
                               DECRIPCION = cc.DECRIPCION
                           };
            return objDatos;
        }

        /// <summary>
        /// Carga la lista del servicio para la validación de la existencia de un servicio
        /// </summary>
        /// <param name="strCodigoServicio">Indica el código del servicio a consultar</param>
        /// <param name="strCodigoSubGrupo">Indica el código del SubGrupo a consultar</param>
        /// <param name="strCodigoGrupo">Indica el código del Grupo o Indicador de Servicio a consultar</param>
        /// <returns>Retorna un objeto JSON que es consumido desde un ajax ejecutado por la vista para la validación de existencia del servicio</returns>
        public ActionResult ConsultarServiciosPorCodigo(string strCodigoServicio, string strCodigoSubGrupo, string strCodigoGrupo)
        {
            var objDatos = from s in db.PS_SERVICIOS
                               //join g in db.GRUPOes on s.COD_GRUPO equals g.COD_GRUPO into jG
                               //from g in jG
                               //join sg in db.SUB_GRUPO on new { s.COD_SUBGRUPO, s.COD_GRUPO } equals new { sg.COD_SUBGRUPO, sg.COD_GRUPO } into jSg
                               //from sg in jSg
                               ////join tc in db.GN_TIPO_TECNOLOGIA on s.ID_TECNOLOGIA equals tc.ID into jTc
                               ////from tc in jTc.DefaultIfEmpty()
                           where (s.COD_SERVICIO.TrimEnd() == strCodigoServicio.TrimEnd()
                                && s.COD_GRUPO.TrimEnd() == strCodigoGrupo.TrimEnd()
                                && s.COD_SUBGRUPO.TrimEnd() == strCodigoSubGrupo.TrimEnd())
                           select new
                           {
                               COD_SERVICIO = s.COD_SERVICIO.TrimEnd(),
                               s.COD_SUBGRUPO,
                               COD_GRUPO = s.COD_GRUPO.TrimEnd(),
                               s.DESCR_SERVICIO,
                               //s.ABREVIATURA,
                               //s.COD_ENTIDAD,
                               //s.COD_CLASIF_CONTABLE,
                               //s.APLICA_ESPECIALIDAD,
                               //s.COD_CONCEPTOC_EVENTO,
                               //s.COD_CONCEPTOC_CAPITA,
                               //s.COD_ENTIDADSP,
                               //s.COMPLEJIDAD,
                               //s.REGISTRO_SANITARIO,
                               //s.COD_TIPO_SERVICIO,
                               //g.DESCR_GRUPO,
                               //sg.DESCR_SUBGRUPO,
                               //s.CUM,
                               //s.ID_ESTADO_SERVICIOS,
                               //s.OBSERVACIONES,
                               //s.FORMULA_MAGISTRAL,
                               //s.VITAL_DISPONIBLE,
                               //s.ID_TECNOLOGIA,
                               //s.CODIGO_IUM,
                               //tc.DESCRIPCION_SERVICIO,
                               //s.AUTORIZACION_RELACIONADA,
                           };

            return Json(objDatos, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Consulta el estado del servicio por parametro seleccionado
        /// </summary>
        /// <param name="intCodigoEstado">Indica el codigo del estado con el cual se creo el servicio</param>
        /// <returns>Retorna un objeto JSON que es consumido desde un ajax ejecutado por la vista</returns>
        public ActionResult ConsultarPorParametro(int intCodigoEstado)
        {
            var objDatos = from svE in db.SV_ESTADO_SERVICIOS
                           where (svE.ID == intCodigoEstado)
                           select new
                           {
                               svE.ID,
                               svE.DECRIPCION
                           };

            return Json(objDatos, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Carga la lista del Regimen de los servicios. Ej "Contributivo, Subsidiado y Mixto"
        /// </summary>
        /// <returns>Retorna un objeto IEnumerable<object> con la lista del regimen del servicio</returns>
        public IEnumerable<object> CargarRegimen()
        {
            var objDatos = from r in db.GN_PLANES
                           where !r.DESC_PLAN.Contains("MIXTO")
                           select new
                           {
                               r.COD_PLAN,
                               r.DESC_PLAN,
                               r.COD_PLAN_NORMA,
                               r.ABREVIAT,
                               //r.PS_DETALLE_SERVICIOS,
                               r.RES_744_2012
                           };
            return objDatos;
        }

        /// <summary>
        /// Carga la lista de la complejidad
        /// </summary>
        /// <returns>Retorna un objeto IEnumerable<object> con la lista de la complejidad</returns>
        public IEnumerable<object> CargarClasificacionPos()
        {
            var objDatos = from PC in db.PS_CLASIFICACION_POS
                           where PC.ESTADO == true
                           select new
                           {
                               PC.ID,
                               PC.DESCRIPCION,
                               PC.ESTADO
                           };

            //return Json(objDatos, JsonRequestBehavior.AllowGet);
            return objDatos;
        }

        /// <summary>
        /// Consulta los diagnosticos por parametro seleccionado
        /// </summary>
        /// <param name="strDescrDiagnostico">Indica la descripción del diagnostico a consultar</param>
        /// <param name="strCodDiagnostico">Indica el código del diagnostico a consultar</param>
        /// <returns>Retorna un objeto JSON que es consumido desde un ajax ejecutado por la vista para la opción de busqueda por diagnostico</returns>
        public ActionResult CargarDiagnostico(string strDescrDiagnostico, string strCodDiagnostico)
        {
            var objDatos = from DG in db.DIAGNOSTICOS
                           where (!string.IsNullOrEmpty(strDescrDiagnostico) ? DG.DESCR_DIAGNOSTICO.Contains(strDescrDiagnostico) : (!string.IsNullOrEmpty(strCodDiagnostico) ? DG.DIAGNOSTICO1 == strCodDiagnostico : true))
                           select new
                           {
                               DIAGNOSTICO = DG.DIAGNOSTICO1,
                               DESCR_DIAGNOSTICO = DG.DESCR_DIAGNOSTICO.TrimEnd(),
                           };

            JsonResult jr = null;
            var jsonData = new
            {
                total = 1,
                page = 1,
                records = objDatos.Count(),
                rows = objDatos.ToArray()
            };
            jr = Json(jsonData, JsonRequestBehavior.AllowGet);
            jr.MaxJsonLength = 900000000;

            //return Json(objDatos.Take(1000), JsonRequestBehavior.AllowGet);
            //return objDatos.Take(100);
            return jr;
        }

        /// <summary>
        /// Carga la lista de los Grupos Etareos
        /// </summary>
        /// <returns>Retorna un objeto IEnumerable<object> con la lista de los grupos etareos</returns>
        public IEnumerable<object> CargarGruposEtareos()
        {
            var objDatos = from GE in db.PS_GRUPOS_ETAREOS
                           where GE.ESTADO_GRUPOETAREO == true
                           select new
                           {
                               GE.ID,
                               GE.DESC_GRUPOETAREO
                           };

            //return Json(objDatos, JsonRequestBehavior.AllowGet);
            return objDatos;
        }

        /// <summary>
        /// Carga la lista de la entidad Sexo
        /// </summary>
        /// <returns>Retorna un objeto IEnumerable<object> con la lista de la entidad Sexo</returns>
        public IEnumerable<object> CargarSexo()
        {
            var objDatos = (from sx in db.SEXOes
                            select new
                            {
                                COD_SEXO = sx.COD_SEXO,
                                DESC_SEXO = sx.DESC_SEXO
                            });

            return objDatos;
        }

        /// <summary>
        /// Carga las listas para los combos con opción de SI y NO
        /// </summary>
        /// <returns>Retorna un objeto IEnumerable<object> con la lista para los combos con opción de SI y NO</returns>
        public List<SelectListItem> CargarListasSiNo()
        {
            var objDatos = new List<SelectListItem>() {
                new SelectListItem
                {
                    Text = "SI",
                    Value = "S",
                    Group = new SelectListGroup() { Name = "Group 1" }
                }, new SelectListItem {
                    Text = "NO",
                    Value = "N",
                    Group = new SelectListGroup() { Name = "Group 2" }
                }
            };

            return objDatos;
        }

        /// <summary>
        /// Carga la lista del combo profesional autoriza
        /// </summary>
        /// <returns>Retorna un objeto IEnumerable<object> con la lista del profesional autoriza</returns>
        public IEnumerable<object> CargarProfesionalAutoriza()
        {
            var objDatos = (from pa in db.SV_PROFESIONAL_AUTORIZA
                            select new
                            {
                                PROFESIONAL_AUTORIZA = pa.PROFESIONAL_AUTORIZA,
                                DESC_PROFESIONAL_AUTORIZA = pa.DESC_PROFESIONAL_AUTORIZA
                            });

            return objDatos;
        }

        /// <summary>
        /// Carga la lista del combo Nivel autorización
        /// </summary>
        /// <returns>Retorna un objeto IEnumerable<object> con la lista del nivel autorización</returns>
        public IEnumerable<object> CargarListaNivelAutorizacion()
        {
            var objDatos = (from na in db.GN_NIVEL_AUTORIZ
                            where na.NIVEL_AUTORIZACION > 0
                            select new
                            {
                                NIVEL_AUTORIZACION = na.NIVEL_AUTORIZACION,
                                DESC_AUTORIZACION = na.DESC_AUTORIZACION,
                                DESC_USUARIO = na.DESC_USUARIO
                            });

            return objDatos;
        }

        /// <summary>
        /// Carga la lista de la entidad Condición
        /// </summary>
        /// <returns>Retorna un objeto JSON que es consumido desde un ajax ejecutado por la vista para la opción de condición en la clasificación del servicio</returns>
        public ActionResult CargarListaCondiciones()
        {
            var objDatos = (from na in db.PS_CONDICIONES
                            select new
                            {
                                DESCRIPCION = na.DESCRIPCION,
                                ID = na.ID,
                                ESTADO = na.ESTADO
                            });

            //return objDatos;
            return Json(objDatos, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Carga la lista de regional para la validación de los regimenes
        /// </summary>
        /// <param name="Cod_Plan">Indica el código del plan o regimen seleccionado</param>
        /// <param name="Cod_Regional">Indica la regional por la cual se está creando el servicio</param>
        /// <returns>Retorna un objeto JSON que es consumido desde un ajax ejecutado por la vista para la busqueda de la regional y el plan por el que esta parametrizado el plan</returns>
        public ActionResult CargarListaAdmin_Regional(string Cod_Plan, string Cod_Regional)
        {
            var objDatos = (from na in db.GN_ADMINXREGIONAL
                            where (!string.IsNullOrEmpty(Cod_Plan) ? na.COD_PLAN.TrimEnd() == Cod_Plan : true)
                                && (!string.IsNullOrEmpty(Cod_Regional) ? na.COD_REGIONAL.TrimEnd() == Cod_Regional : true)
                            select new
                            {
                                COD_EPS = na.COD_EPS,
                                COD_REGIONAL = na.COD_REGIONAL,
                                COD_PLAN = na.COD_PLAN
                            });

            //return objDatos;
            return Json(objDatos, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Enumerador para la funcion UrlServicioPaquete
        /// </summary>
        private enum eProceso_Origen
        {
            Nuevo_Servicio = 0,
            Actualiza_Servicio = 1
        }

        /// <summary>
        /// Función utilizada para cuando en un servicio se selecciona el indicador de servicio PAQUETE
        /// </summary>
        /// <param name="strCodigoServicio">Indica el código del servicio a consultar</param>
        /// <param name="strCodigoSubGrupo">Indica el código del SubGrupo a consultar</param>
        /// <param name="strCodigoGrupo">Indica el código del Grupo o Indicador de Servicio a consultar</param>
        /// <param name="COD_PLAN">Indica el código del plan seleccionado cuando el servicio es NO POS</param>
        /// <param name="COD_EPS">Indica el código de la eps seleccionada cuando el servicio es NO POS</param>
        /// <param name="COD_REGIONAL">Indica el código de la regional seleccionada cuando el servicio es NO POS</param>
        /// <param name="UrlReferer">Indica la url de donde se esta haciendo el llamado al servicio Rest</param>
        /// <param name="intProceso_Origen">Indici el enumerador</param>
        /// <returns>retorna la URL donde se va a redirigir cuando es un paquete</returns>
        public ActionResult UrlServicioPaquete(string strCodigoServicio, string strCodigoSubGrupo, string strCodigoGrupo, string COD_PLAN, string COD_EPS, string COD_REGIONAL, string UrlReferer, int intProceso_Origen = (int)eProceso_Origen.Nuevo_Servicio)
        {
            var strUrl = "";
            var strMensaje = "";

            strUrl = strUrl + "strCodigoServicio=" + strCodigoServicio.TrimEnd();
            strUrl = strUrl + "&";
            strUrl = strUrl + "strCodigoSubGrupo=" + strCodigoSubGrupo.TrimEnd();
            strUrl = strUrl + "&";
            strUrl = strUrl + "strCodigoGrupo=" + strCodigoGrupo.TrimEnd();
            strUrl = strUrl + "&";
            strUrl = strUrl + "strCodigoPlan=" + COD_PLAN;
            strUrl = strUrl.TrimEnd() + "&";
            strUrl = strUrl.TrimEnd() + "strCodigoEPS=" + COD_EPS;
            strUrl = strUrl.TrimEnd() + "&";
            strUrl = strUrl.TrimEnd() + "strCodigoRegional=" + COD_REGIONAL;
            strUrl = strUrl.TrimEnd() + "&";
            //'Inicio: ppacheco_20131021_1720
            //'strUrl = strUrl.Trim & "strParamSiNo=" & "S"
            strUrl = strUrl.TrimEnd() + "strParamSiNo=" + "N";
            //'Fin: ppacheco_20131021_1720
            strUrl = strUrl.TrimEnd() + "&";
            strUrl = strUrl.TrimEnd() + "intProceso_Origen=" + intProceso_Origen;
            strUrl = strUrl.TrimEnd();

            if (intProceso_Origen == (int)eProceso_Origen.Nuevo_Servicio)
            {
                switch (strCodigoGrupo.TrimEnd())
                {
                    case "PAQ":
                        strUrl = UrlReferer + "PS_Paquetes_Servicios.aspx?" + strUrl;
                        strMensaje = "El servicio fué creado exitosamente, no olvide parametrizar el Detalle del Paquete..!";
                        break;
                    default:
                        strUrl = UrlReferer + "PS_Nuevo_Parametrizacion_Servicio.aspx?" + strUrl;
                        strMensaje = "El servicio fué creado exitosamente, no olvide actualizar" + "\r" + " la parametrización del servicio..!";
                        break;
                }
            } else
            {
                strUrl = UrlReferer + "PS_Paquetes_Servicios.aspx?" + strUrl;
            }

            return Json(new JsonResult { Data = new { strMensaje = strMensaje, strUrl = strUrl, Ok = true } }, JsonRequestBehavior.AllowGet);
        }
    }
}
