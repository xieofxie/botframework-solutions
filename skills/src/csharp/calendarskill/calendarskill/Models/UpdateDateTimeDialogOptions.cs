﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

namespace CalendarSkill.Models
{
    public class UpdateDateTimeDialogOptions : CalendarSkillDialogOptions
    {
        public UpdateDateTimeDialogOptions()
        {
            Reason = UpdateReason.NotFound;
        }

        public UpdateDateTimeDialogOptions(UpdateReason reason, object skillOptions)
            : base(skillOptions)
        {
            Reason = reason;
        }

        public enum UpdateReason
        {
            /// <summary>
            /// NotADateTime.
            /// </summary>
            NotADateTime,

            /// <summary>
            /// NotFound.
            /// </summary>
            NotFound,

            /// <summary>
            /// NoEvent.
            /// </summary>
            NoEvent,
        }

        public UpdateReason Reason { get; set; }
    }
}