using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Eperformance1
{
    public class UserGroup
    {
        public int GroupId { get; set; }
        public string Group { get; set; }

        public bool AddKPI { get; set; }
        public bool EditKPI { get; set; }
        public bool DeleteKPI { get; set; }
        public bool ViewKPI { get; set; }

        public bool AddKRA { get; set; }
        public bool EditKRA { get; set; }
        public bool DeleteKRA { get; set; }
        public bool ViewKRA { get; set; }

        public bool AddEPMaster { get; set; }
        public bool EditEPMaster { get; set; }
        public bool DeleteEPMaster { get; set; }
        public bool ViewEPMaster { get; set; }

        public bool EditMidYrAppraisee	{ get; set; }
        public bool SubmitMidYrAppraisee { get; set; }

        public bool ViewMidYrAppraisee	{ get; set; }

        public bool EditMidYrAppraisal	{ get; set; }
        public bool SubmitMidYrAppraisal { get; set; }
        public bool ViewMidYrAppraisal { get; set; }

        public bool AddProjects { get; set; }
        public bool EditProjects { get; set; }
        public bool DeleteProjects { get; set; }
        public bool ViewProjects { get; set; }

        public bool SendMail { get; set; }

        public bool SetApprovalOrder { get; set; }

        public bool AddMeeting { get; set; }
        public bool EditMeeting { get; set; }
        public bool DeleteMeeting { get; set; }
        public bool ViewMeeting { get; set; }

        public bool MailExternalAttachments { get; set; }
        public bool EditExternalAttachments { get; set; }
        public bool DeleteExternalAttachments { get; set; }
        public bool ViewExternalAttachments { get; set; }

        public bool AddRemarks { get; set; }
        public bool EditRemarks { get; set; }
        public bool DeleteRemarks { get; set; }
        public bool ViewRemarks { get; set; }

        public bool DocumentClose { get; set; }

        public bool AddRFI { get; set; }
        public bool EditRFI { get; set; }
        public bool DeleteRFI { get; set; }
        public bool ViewRFI { get; set; }

        public bool EditPlanOfApprove { get; set; }
        public bool ApprovePlanOfApprove { get; set; }
        public bool ViewPlanOfApprove { get; set; }


        public bool EditApproval { get; set; }
        public bool ViewApproval { get; set; }

        public bool ViewConfiguration { get; set; }

        public bool AddCO { get; set; }
        public bool EditCO { get; set; }
        public bool DeleteCO { get; set; }
        public bool ViewCO { get; set; }
    }

}