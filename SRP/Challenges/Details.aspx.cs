﻿using GRA.SRP.Controls;
using GRA.SRP.DAL;
using GRA.SRP.Utilities.CoreClasses;
using GRA.Tools;
using SRPApp.Classes;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace GRA.SRP.Challenges
{
    public partial class Details : BaseSRPPage
    {
        private bool ProgramOpen { get; set; }
        private bool ChallengeCompleted { get; set; }
        private readonly string BadgeLink = VirtualPathUtility.ToAbsolute("~/Badges/Details.aspx");
        private const string BadgeLinkAndImage = "<a href=\"{0}?BadgeId={1}\" runat=\"server\" OnClick=\"return ShowBadgeInfo({1});\" class=\"thumbnail pull-left\"><img src=\"/images/badges/sm_{1}.png\" /></a>";
        public bool ShowModal { get; set; }

        protected string ShowBadge(object badgeIdObject)
        {
            int? badgeId = badgeIdObject as int?;
            if (badgeId == null || badgeId == 0)
            {
                return "No badge.";
            }
            else
            {
                var badge = DAL.Badge.FetchObject((int)badgeId);
                if (badge == null)
                {
                    return "No badge.";
                }
                if (badge.HiddenFromPublic != true)
                {
                    return string.Format(BadgeLinkAndImage, BadgeLink, badgeId);
                }
                else
                {
                    return "Unknown <small>(it's a secret)</small>.";
                }
            }
        }
        protected string ProgressDisplay(object amountObject, object totalObject)
        {
            int amount = amountObject as int? ?? 0;
            int total = totalObject as int? ?? 0;

            if (total == 0 || amount == 0)
            {
                return string.Empty;
            }
            if (amount == total)
            {
                return "Challenge complete!";
            }

            return string.Format("{0} of {1}", amountObject, totalObject);
        }

        protected int ComputePercent(object amountObject, object totalObject)
        {
            int amount = amountObject as int? ?? 0;
            int total = totalObject as int? ?? 0;

            if (total == 0)
            {
                return 100;
            }
            else
            {
                return (int)(amount * 100.0 / total);
            }
        }

        protected void Page_Init(object sender, EventArgs e)
        {
            Programs pgm = null;
            var patron = Session[SessionKey.Patron] as Patron;
            if (patron != null)
            {
                pgm = DAL.Programs.FetchObject(patron.ProgID);
                if (pgm == null)
                {
                    pgm = Programs.FetchObject(
                        Programs.GetDefaultProgramForAgeAndGrade(patron.Age,
                                                                 patron.SchoolGrade.SafeToInt()));
                }
            }


            if (pgm == null || !pgm.IsOpen || ChallengeCompleted)
            {
                this.ProgramOpen = false;
                btnSave.Visible = false;
            }
            else
            {
                this.ProgramOpen = true;
                btnSave.Visible = true;
            }

            if (this.PrintPage.Equals("true", StringComparison.OrdinalIgnoreCase))
            {
                btnSave.Visible = false;
            }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                if (Request.QueryString["ChallengeId"] != null)
                {
                    int blid;
                    if (int.TryParse(Request.QueryString["ChallengeId"].ToString(), out blid))
                    {
                        LookupChallenge(blid);
                    }

                }
                else
                {
                    Response.Redirect("~/Challenges/");
                }
                challengesBackLink.NavigateUrl = "~/Challenges/";
                TranslateStrings(this);
            }
        }

        protected void rptr_ItemDataBound(object source, RepeaterItemEventArgs e)
        {
            if (!this.ProgramOpen
                || ChallengeCompleted
                || this.PrintPage.Equals("true", StringComparison.OrdinalIgnoreCase))
            {
                if (e.Item.ItemType == ListItemType.Item
                   || e.Item.ItemType == ListItemType.AlternatingItem)
                {
                    var checkbox = e.Item.FindControl("chkRead") as CheckBox;
                    if (checkbox != null)
                    {
                        checkbox.Enabled = false;
                    }

                }
            }
        }

        protected void LookupChallenge(int blid)
        {
            var bl = BookList.FetchObject(blid);

            if (bl == null)
            {
                challengeDetails.Visible = false;
                new SessionTools(Session).AlertPatron("Could not find details on that Challenge.",
                    PatronMessageLevels.Warning,
                    "exclamation-sign");
            }
            else
            {
                int patronId = -1;
                var p = Session[SessionKey.Patron] as Patron;
                if (p != null)
                {
                    patronId = p.PID;
                }

                // see if this is bound to a specific program
                if (bl.ProgID != 0)
                {
                    // no user is logged in, don't show it
                    if (p == null)
                    {
                        var prog = DAL.Programs.FetchObject(bl.ProgID);
                        challengeDetails.Visible = false;
                        new SessionTools(Session).AlertPatron(
                            string.Format("You must be registered in the <strong>{0}</strong> program to view this Challenge.",
                                prog.TabName),
                            PatronMessageLevels.Warning,
                            "exclamation-sign");
                    }

                    // user is registered under another program
                    if (p != null && bl.ProgID != p.ProgID)
                    {
                        var prog = DAL.Programs.FetchObject(bl.ProgID);
                        challengeDetails.Visible = false;
                        new SessionTools(Session).AlertPatron(
                            string.Format("That Challenge is only available to people in the <strong>{0}</strong> program.",
                                prog.TabName),
                            PatronMessageLevels.Warning,
                            "exclamation-sign");
                    }
                }

                if (challengeDetails.Visible)
                {
                    Badge badge = null;

                    challengeTitle.Text = bl.ListName;
                    this.Title = string.Format("Challenge: {0}", challengeTitle.Text);
                    lblDesc.Text = string.Format("<p>{0}</p>", Server.HtmlDecode(bl.Description));

                    string award = null;

                    if (bl.AwardPoints > 0)
                    {
                        award = string.Format("Completing <strong>{0} task{1}</strong> will earn: <strong>{2} point{3}</strong>",
                            bl.NumBooksToComplete,
                            bl.NumBooksToComplete > 1 ? "s" : string.Empty,
                            bl.AwardPoints,
                            bl.AwardPoints > 1 ? "s" : string.Empty);
                    }

                    if (bl.AwardBadgeID > 0)
                    {
                        badge = DAL.Badge.FetchObject(bl.AwardBadgeID);
                        if (badge != null)
                        {
                            if (badge.HiddenFromPublic != true)
                            {
                                if (string.IsNullOrWhiteSpace(award))
                                {
                                    award = string.Format("Completing {0} task{1} will earn: <strong>a badge</strong>.",
                                        bl.NumBooksToComplete,
                                        bl.NumBooksToComplete > 1 ? "s" : string.Empty);
                                }
                                else
                                {
                                    award += " and <strong>a badge</strong>.";
                                }

                                BadgeImage.Text = string.Format("<img class=\"thumbnail disabled\" src=\"/images/badges/sm_{0}.png\" />", bl.AwardBadgeID);
                            }
                            else
                            {
                                badge = null;
                                if (string.IsNullOrWhiteSpace(award))
                                {
                                    award = string.Format("Completing {0} task{1} will earn: <strong>a secret badge</strong>.",
                                        bl.NumBooksToComplete,
                                        bl.NumBooksToComplete > 1 ? "s" : string.Empty);
                                }
                                else
                                {
                                    award += " and <strong>a secret badge</strong>.";
                                }
                                BadgeImage.Text = string.Empty;
                            }
                        }
                    }
                    else
                    {
                        BadgeImage.Text = string.Empty;
                        award += ".";
                    }

                    BadgeImage.Visible = !string.IsNullOrEmpty(BadgeImage.Text);

                    if (!string.IsNullOrWhiteSpace(award))
                    {
                        lblPoints.Text = award;
                        lblPoints.Visible = true;
                    }

                    var ds = BookListBooks.GetForDisplay(bl.BLID, patronId);

                    //Eval("NumBooksCompleted"), Eval("NumBooksToComplete")
                    if(ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
                    {
                        int completed = 0;
                        foreach(DataRow row in ds.Tables[0].Rows) {
                            if ((bool?)row["HasRead"] == true)
                            {
                                completed++;
                                if(completed >= bl.NumBooksToComplete)
                                {
                                    ChallengeCompleted = true;
                                    break;
                                }
                            }
                        }
                    }

                    rptr.DataSource = ds;
                    rptr.DataBind();

                    // begin social
                    var wt = new WebTools();

                    string systemName = StringResources.getStringOrNull("system-name");
                    var fbDescription = StringResources.getStringOrNull("facebook-description");
                    var hashtags = StringResources.getStringOrNull("socialmedia-hashtags");

                    string title = string.Format("{0} challenge: {1}",
                        systemName,
                        bl.ListName);

                    string description = string.Format("Check out this {0} challenge: {1}!",
                        systemName,
                        bl.ListName);

                    string twitDescrip = description;
                    if (twitDescrip.Length > 118)
                    {
                        // if it's longer than this it won't fit with the url, shorten it
                        twitDescrip = string.Format("Check this out: {0}!",
                            bl.ListName);
                    }

                    var baseUrl = WebTools.GetBaseUrl(Request);
                    var challengeUrl = string.Format("{0}/Challenges/Details.aspx?ChallengeId={1}",
                        baseUrl,
                        bl.BLID);

                    string imagePath = null;
                    if (badge != null)
                    {
                        string potentialBadgePath = string.Format("~/Images/Badges/{0}.png",
                                          badge.BID);
                        if (System.IO.File.Exists(Server.MapPath(potentialBadgePath)))
                        {
                            imagePath = string.Format("{0}{1}",
                                baseUrl,
                                VirtualPathUtility.ToAbsolute(potentialBadgePath));
                        }
                    }

                    if (string.IsNullOrEmpty(imagePath))
                    {
                        imagePath = new GRA.Logic.Banner().FullMetadataBannerPath(baseUrl,
                            Session,
                            Server);
                    }

                    wt.AddOgMetadata(Metadata,
                        title,
                        wt.BuildFacebookDescription(description, hashtags, fbDescription),
                        imagePath,
                        challengeUrl,
                        facebookApp: GetResourceString("facebook-appid"));

                    wt.AddTwitterMetadata(Metadata,
                        title,
                        description,
                        imagePath,
                        twitterUsername: GetResourceString("twitter-username"));

                    TwitterShare.NavigateUrl = wt.GetTwitterLink(description,
                        Server.UrlEncode(challengeUrl),
                        hashtags);
                    TwitterShare.Visible = true;
                    FacebookShare.NavigateUrl = wt.GetFacebookLink(Server.UrlEncode(challengeUrl));
                    FacebookShare.Visible = true;
                    // end social

                    this.ShowModal = true;
                }
            }
        }

        protected void btnSave_Click(object sender, EventArgs e)
        {
            if (!this.ProgramOpen)
            {
                return;
            }

            var now = DateTime.Now;
            var onlyCheckedBoxes = true;
            var selBLI = 0;
            var readCount = 0;
            var neeedCount = 0;
            var BLID = -1;
            foreach (RepeaterItem item in rptr.Items)
            {
                if (item.ItemType == ListItemType.Item || item.ItemType == ListItemType.AlternatingItem)
                {
                    if (BLID < 0)
                    {
                        BLID = int.Parse(((Label)item.FindControl("BLID")).Text);
                        neeedCount = BookList.FetchObject(BLID).NumBooksToComplete;
                    }
                    var chkRead = (CheckBox)item.FindControl("chkRead");
                    var PBLBID = int.Parse(((Label)item.FindControl("PBLBID")).Text);
                    var BLBID = int.Parse(((Label)item.FindControl("BLBID")).Text);

                    selBLI = BLID;
                    var pbl = new PatronBookLists();
                    if (PBLBID != 0)
                    {
                        pbl = PatronBookLists.FetchObject(PBLBID);
                    }
                    pbl.BLBID = BLBID;
                    pbl.BLID = BLID;
                    pbl.PID = ((Patron)Session[SessionKey.Patron]).PID;
                    pbl.LastModDate = now;

                    pbl.HasReadFlag = chkRead.Checked;
                    if (!pbl.HasReadFlag)
                    {
                        onlyCheckedBoxes = false;
                    }
                    else
                    {
                        readCount++;
                    }

                    if (PBLBID != 0)
                    {
                        pbl.Update();
                    }
                    else
                    {
                        pbl.Insert();
                    }
                }
            }

            new SessionTools(Session).AlertPatron("Your progress has been saved!",
                glyphicon: "check");

            // read the entire book list!  Award points and badges 
            if ((neeedCount == 0 && onlyCheckedBoxes) || (neeedCount <= readCount))
            {
                new SessionTools(Session).AlertPatron("Good work, you've completed a Challenge!",
                    glyphicon: "star");

                var bl = BookList.FetchObject(selBLI);

                if (PatronPoints.HasEarnedBookList(((Patron)Session[SessionKey.Patron]).PID, selBLI))
                {
                    return;
                }

                if (bl.AwardBadgeID != 0 || bl.AwardPoints != 0)
                {
                    new SessionTools(Session).AlertPatron("Congratulations, you completed a Challenge and were awarded a badge!",
                        glyphicon: "certificate");

                    var pa = new AwardPoints(((Patron)Session[SessionKey.Patron]).PID);
                    var sBadges = pa.AwardPointsToPatron(bl.AwardPoints, PointAwardReason.BookListCompletion,
                                                            bookListID: bl.BLID);
                    if (sBadges.Length > 0)
                    {
                        new SessionTools(Session).EarnedBadges(sBadges);
                    }
                }
            }
        }

    }
}