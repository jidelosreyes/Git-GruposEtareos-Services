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
    
    public partial class GN_USUARIO_SISTEMA
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public GN_USUARIO_SISTEMA()
        {
            this.PS_GRUPOS_ETAREOS = new HashSet<PS_GRUPOS_ETAREOS>();
            this.PS_DETALLE_SERVICIOS = new HashSet<PS_DETALLE_SERVICIOS>();
        }
    
        public string COD_USUARIO { get; set; }
        public string NOMBRE_USUARIO { get; set; }
        public string CLAVE_USUARIO { get; set; }
        public string COD_IPS { get; set; }
        public string COD_SEDE_IPS { get; set; }
        public int NIVEL_AUTORIZACION_3 { get; set; }
        public string ESTADO { get; set; }
        public string COD_REGIONAL { get; set; }
        public string COD_EPS { get; set; }
        public string TIPO_USUARIO { get; set; }
        public string COD_PLAN { get; set; }
        public string PROFESIONAL_AUTORIZA { get; set; }
        public string ARCHIVO_FIRMA { get; set; }
        public byte[] FIRMA { get; set; }
        public string COD_COBERTURA { get; set; }
        public string E_MAIL { get; set; }
        public string Telefono { get; set; }
        public Nullable<bool> Atender_Caso_SNE { get; set; }
        public Nullable<int> Id_EstJerarquica { get; set; }
        public int COD_CARGO { get; set; }
        public string USER_CALL_CENTER { get; set; }
        public bool AUDITOR_FALLO_TUTELA { get; set; }
        public bool ANULA_PRESTACIONES_ECONOMICAS { get; set; }
        public short COD_SERVIDOR { get; set; }
        public Nullable<bool> MEDICO_AUDITOR { get; set; }
        public Nullable<bool> COORDINADOR_AC { get; set; }
        public Nullable<int> COD_ROLE { get; set; }
        public Nullable<bool> ANULA_ACTAS_CONCILIACION { get; set; }
        public Nullable<bool> ANULA_TUTELAS { get; set; }
        public Nullable<bool> AUTORIZA_PGP { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<PS_GRUPOS_ETAREOS> PS_GRUPOS_ETAREOS { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<PS_DETALLE_SERVICIOS> PS_DETALLE_SERVICIOS { get; set; }
    }
}
