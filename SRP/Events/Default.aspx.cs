﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using SRPApp.Classes;
using GRA.SRP.DAL;

namespace GRA.SRP
{
    public partial class Events : BaseSRPPage
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if(!String.IsNullOrEmpty(Request["PID"])) {
                Session["ProgramID"] = Request["PID"].ToString();
            }
            if(!IsPostBack) {
                if(Session["ProgramID"] == null) {
                    try {
                        int PID = Programs.GetDefaultProgramID();
                        Session["ProgramID"] = PID.ToString();
                    } catch {
                        Response.Redirect("~/Default.aspx");
                    }
                }
            }
            TranslateStrings(this);

            //IsSecure = true;
            //if (!IsPostBack) TranslateStrings(this);
        }
    }
}