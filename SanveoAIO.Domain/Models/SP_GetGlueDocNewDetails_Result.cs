//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace SanveoAIO.Domain.Models
{
    using System;
    
    public partial class SP_GetGlueDocNewDetails_Result
    {
        public int Id { get; set; }
        public string GlueFolderPath { get; set; }
        public string GlueFolderID { get; set; }
        public string GlueFileName { get; set; }
        public Nullable<int> GlueVersion { get; set; }
        public string DocFolderPath { get; set; }
        public string DocFolderID { get; set; }
        public string DocFileName { get; set; }
        public string TagName { get; set; }
        public string MappingName { get; set; }
    }
}
