//------------------------------------------------------------------------------
// <auto-generated>
//     Este código se generó a partir de una plantilla.
//
//     Los cambios manuales en este archivo pueden causar un comportamiento inesperado de la aplicación.
//     Los cambios manuales en este archivo se sobrescribirán si se regenera el código.
// </auto-generated>
//------------------------------------------------------------------------------

namespace GruposEtareos.BI
{
    using System;
    using System.Collections.Generic;
    
    public partial class PS_DETALLE_SERVICIOS
    {
        public long ID { get; set; }
        public string COD_SERVICIO { get; set; }
        public string COD_GRUPO { get; set; }
        public string COD_SUBGRUPO { get; set; }
        public string COD_PLAN { get; set; }
        public string COD_EPS { get; set; }
        public string COD_REGIONAL { get; set; }
        public Nullable<long> ID_PS_POS_CONDICIONADO { get; set; }
        public long ID_PS_CLASIFICACION_POS { get; set; }
        public string COD_USUARIO { get; set; }
    
        public virtual GN_ADMINXREGIONAL GN_ADMINXREGIONAL { get; set; }
        public virtual GN_PLANES GN_PLANES { get; set; }
        public virtual GN_USUARIO_SISTEMA GN_USUARIO_SISTEMA { get; set; }
        public virtual PS_CLASIFICACION_POS PS_CLASIFICACION_POS { get; set; }
        public virtual PS_POS_CONDICIONADO PS_POS_CONDICIONADO { get; set; }
        public virtual PS_SERVICIOS PS_SERVICIOS { get; set; }
    }
}
