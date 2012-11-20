﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using InspectR.Controllers;
using InspectR.Core;
using InspectR.Data;
using Microsoft.AspNet.SignalR.Hubs;

namespace InspectR.Hubs
{
    public class InspectRHub : Hub
    {
        private IRequestCache _requestCache;
        private InspectRContext _dbContext;
        private IInspectRService _service;

        public InspectRHub()
        {
            _requestCache = new RequestCache();
            _dbContext = new InspectRContext();
            _service = new InspectRService(_dbContext);
        }

        public InspectorInfo StartInspect(string inspector)
        {
            var info = _dbContext.GetInspectorInfoByKey(inspector);
            if (info == null)
                return null;

            if (Context.User != null)
            {
                var user = Context.User.Identity.Name;
                if (!string.IsNullOrEmpty(user))
                {
                    _service.AddInspectorToUser(user, info);
                }
            }

            Groups.Add(Context.ConnectionId, info.UniqueKey);

            return info;
        }

        public InspectRUserProfile GetUserProfile()
        {
            if (Context.User != null)
            {
                var username = Context.User.Identity.Name;
                if (!string.IsNullOrEmpty(username))
                {
                    // todo: map a dto
                    return _dbContext.GetUserProfile(username);
                }
            }

            return null;
        }

        public IEnumerable<RequestInfo> GetRecentRequests(string inspector)
        {
            InspectorInfo inspectorInfo = _dbContext.GetInspectorInfoByKey(inspector);
            var recentRequests = _requestCache.Get(inspectorInfo).OrderByDescending(x=>x.DateCreated).Take(20);
            return recentRequests;
        }

        public void ClearRecentRequests(string inspector)
        {
            InspectorInfo inspectorInfo = _dbContext.GetInspectorInfoByKey(inspector);
            _requestCache.RemoveAll(inspectorInfo);
        }

        public void SetTitle(Guid id, string title)
        {
            if (string.IsNullOrWhiteSpace(title))
            {
                return;
            }

            InspectorInfo inspectorInfo = _dbContext.GetInspectorInfo(id);
            inspectorInfo.Title = title;
            _dbContext.SaveChanges();
        }

        public override System.Threading.Tasks.Task OnConnected()
        {
            return base.OnConnected();
        }
    }
}