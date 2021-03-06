﻿using Project.Models;
using Project.Web.Data;
using System.Linq;
using System;
using System.Collections.Generic;
using System.Text;
using Project.Services.Contracts;
using Project.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace Project.Services
{
    public class JobService:IJobService
    {
        private ApplicationDbContext context;

        public JobService(ApplicationDbContext context)
        {
            this.context = context;
        
        }

        public void CreateJob(string title, string description, string address, string username,
             string categoryName)
        {
            var user = this.context.Users
                .Include(x=> x.UserProfile)
                .FirstOrDefault(x=> x.UserName == username);

            var category = this.context.Categories
                .FirstOrDefault(x => x.Name == categoryName);

            var job = new Job()
            {
                Title = title,
                Description = description,
                
                Address = address,
                User = user.UserProfile,
                UserId = user.UserProfile.Id,
                Category = category,
                CategoryId = category.Id,
            };

            this.context.Jobs.Add(job);
            this.context.SaveChanges();
        }

        public IEnumerable<Job> GetJobs(string username,JobStatus status)
        {

            var jobs = this.context.Jobs.Where(x => x.User.Account.UserName == username).Where(x => x.Status == status).ToList();

            return jobs;
        }

        public IEnumerable<Job> GetCompanyJobs(string username, JobStatus status)
        {
            var jobs = this.context.Jobs.Where(x => x.Company.Account.UserName == username).Where(x => x.Status == status).ToList();

            return jobs;
        }



        public Job GetJob(string id)
        {
            var job = this.context.Jobs.Include(x=> x.Company)
                .Include(x=> x.Category)
                .Include(x=> x.User)
                .Include(x=> x.Contract)
                .Include(x=> x.User.Account)
                .FirstOrDefault(x => x.Id == id);

            return job;
        }

        public IEnumerable<Job> GetWaitingForCompanyJobsWithSameCategories(string companyUsername)
        {
            var company = this.context.CompaniesProfiles
                .Include(x=>x.Account)
                .Include(x => x.Categories)
                .FirstOrDefault(x => x.Account.UserName == companyUsername);

            var jobs = this.context.Jobs
                .Where(x => company.Categories.Contains(x.Category))
                .Where(x=> x.Status == JobStatus.WaitingForCompany)
                .ToList();

            return jobs;
        }

        public bool AcceptOffer(Offer offer)
        {
            var job =this.context.Jobs.FirstOrDefault(x => x.Id == offer.JobId);

            if (job == null)
            {
                return false;
            }

            job.StartDate = offer.StartDate;
            job.EndDate = offer.EndDate;
            job.CompanyId = offer.CompanyId;
            job.Company = offer.Company;
            job.Price = offer.Price;
            job.Status = JobStatus.InProgress;

            this.context.SaveChanges();

            return true;
        }

        public void FinishJobs(string username)
        {
           var jobs = this.GetJobs(username, JobStatus.InProgress);

            var finishedJobs = jobs.Where(x => x.EndDate < DateTime.UtcNow).ToList();

            foreach (var job in finishedJobs)
            {
                job.Status = JobStatus.Finished;
            }

            context.SaveChanges();
        }

        public void FinishCompanyJobs(string username)
        {
            var jobs = this.GetCompanyJobs(username, JobStatus.InProgress);

            var finishedJobs = jobs.Where(x => x.EndDate < DateTime.UtcNow).ToList();

            foreach (var job in finishedJobs)
            {
                job.Status = JobStatus.Finished;
            }

            context.SaveChanges();
        }
    }
}
