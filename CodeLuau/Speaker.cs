﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace CodeLuau
{
    /// <summary>
    /// Represents a single speaker
    /// </summary>
    public class Speaker
    {
        private readonly List<string> OldTechnologies = new List<string> { "Cobol", "Punch Cards", "Commodore", "VBScript" };

        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public int? YearsExperience { get; set; }
        public bool HasBlog { get; set; }
        public string BlogURL { get; set; }
        public WebBrowser Browser { get; set; }
        public List<string> Certifications { get; set; }
        public string Employer { get; set; }
        public int RegistrationFee { get; set; }
        public List<Session> Sessions { get; set; }

        /// <summary>
        /// Register a speaker
        /// </summary>
        /// <returns>speakerID</returns>
        public RegisterResponse Register(IRepository repository)
        {
            var error = ValidateRegistration();
            if (error != null) return new RegisterResponse(error);

            var speakerId = repository.SaveSpeaker(this);
            return new RegisterResponse(speakerId);
        }

        private RegisterError? ValidateRegistration()
        {
            var error = ValidateData();
            if (error != null) return error;

            bool speakerAppearsQualified = AppearsExceptional() || !HasObviousRedFlags();
            if (!speakerAppearsQualified) return RegisterError.SpeakerDoesNotMeetStandards;

            var atLeastOneSessionApproved = ApproveSessions();
            if (!atLeastOneSessionApproved) return RegisterError.NoSessionsApproved;

            return null;
        }

        private RegisterError? ValidateData()
        {
            if (string.IsNullOrWhiteSpace(FirstName)) return RegisterError.FirstNameRequired;
            if (string.IsNullOrWhiteSpace(LastName)) return RegisterError.LastNameRequired;
            if (string.IsNullOrWhiteSpace(Email)) return RegisterError.EmailRequired;
            if (!Sessions.Any()) return RegisterError.NoSessionsProvided;
            return null;
        }

        private bool AppearsExceptional()
        {
            if (YearsExperience > 10) return true;
            if (HasBlog) return true;
            if (Certifications.Count() > 3) return true;

            var preferredEmployers = new List<string>() { "Pluralsight", "Microsoft", "Google" };
            if (preferredEmployers.Contains(Employer)) return true;
            return false;
        }

        private bool HasObviousRedFlags()
        {
            string emailDomain = Email.Split('@').Last();
            var ancientEmailDomains = new List<string>() { "aol.com", "prodigy.com", "compuserve.com" };
            if (!ancientEmailDomains.Contains(emailDomain)) return true;
            if (!(Browser.Name == WebBrowser.BrowserName.InternetExplorer && Browser.MajorVersion < 9)) return true;
            return false;
        }

        private bool ApproveSessions()
        {
            foreach (var session in Sessions)
                session.Approved = !SessionIsAboutOldTechnology(session);

            return Sessions.Any(s => s.Approved);
        }

        private bool SessionIsAboutOldTechnology(Session session)
        {
            foreach (var tech in OldTechnologies)
            {
                if (session.Title.Contains(tech) || session.Description.Contains(tech))
                    return true;
            }
            return false;
        }
    }
}