﻿using System;

namespace Contracts.Types.Course
{
    public class RequirementStatus
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public bool Status { get; set; }
    }
}