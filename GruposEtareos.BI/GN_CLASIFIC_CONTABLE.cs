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
    
    public partial class GN_CLASIFIC_CONTABLE
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public GN_CLASIFIC_CONTABLE()
        {
            this.PS_SERVICIOS = new HashSet<PS_SERVICIOS>();
        }
    
        public string COD_CLASIF_CONTABLE { get; set; }
        public string DESC_CONTABLE { get; set; }
        public string PRIV_CAP { get; set; }
        public string PUB_CAP { get; set; }
        public string PRIV_EV { get; set; }
        public string PRIV_PARCIAL { get; set; }
        public string PUB_PARCIAL { get; set; }
        public string COD_AMBITO { get; set; }
        public Nullable<int> PRIORIDAD { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<PS_SERVICIOS> PS_SERVICIOS { get; set; }
    }
}
