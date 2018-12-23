﻿using Project.Models;
using Project.Models.Enums;
using System.Collections.Generic;

namespace Project.Services.Contracts
{
    public interface IJobService
    {
        void CreateJob(string title, string description, decimal maxPrice, string address,string username,
            string categoryName);

        IEnumerable<Job> GetJobs(string username, JobStatus status);

        Job GetJob(string id);
    }
}