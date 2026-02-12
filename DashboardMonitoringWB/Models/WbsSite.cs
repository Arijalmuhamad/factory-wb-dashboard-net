using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DashboardMonitoringWB.Models
{
    [Table("wbs_site_tab")] // Nama table di database
	public class WbsSite
    {
       [Key] // Anggap saja ada primary key
       public string siteid { get; set; }
       public string sitename { get; set; }
	}
}
