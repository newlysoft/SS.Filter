﻿using System;
using System.Collections.Generic;
using System.Linq;
using SiteServer.Plugin;
using SS.Filter.Core;
using SS.Filter.Model;
using SS.Filter.Provider;

namespace SS.Filter.Controllers
{
    public static class FieldsController
    {
        public const string Name = "fields";

        public static List<FieldInfo> List(IRequest request)
        {
            var siteId = request.GetQueryInt("siteId");
            if (siteId == 0)
            {
                throw new Exception("参数不正确：siteId");
            }
            var channelId = request.GetQueryInt("channelId");
            var contentId = request.GetQueryInt("contentId");

            var fieldInfoList = FieldDao.GetFieldInfoList(siteId);
            foreach (var fieldInfo in fieldInfoList)
            {
                fieldInfo.TagInfoList = TagDao.GetTagInfoList(fieldInfo.Id, 0);
                fieldInfo.Tags = fieldInfo.TagInfoList.Select(x => x.Title).ToList();
                fieldInfo.CheckedTagIds = ValueDao.GetTagIdList(siteId, channelId, contentId, fieldInfo.Id);
            }

            return fieldInfoList;
        }

        public static FieldInfo Create(IRequest request)
        {
            var siteId = request.GetQueryInt("siteId");
            if (siteId == 0)
            {
                throw new Exception("参数不正确：siteId");
            }

            if (!request.IsAdminLoggin)
            {
                throw new Exception("未授权请求");
            }

            var fieldInfo = request.GetPostObject<FieldInfo>();

            FieldDao.Insert(siteId, fieldInfo);

            var fieldId = fieldInfo.Id;
            var parentId = 0;

            if (fieldInfo.Tags == null)
            {
                fieldInfo.Tags = new List<string>();
            }

            var taxis = 1;
            foreach (var tag in fieldInfo.Tags)
            {
                TagDao.Insert(fieldId, parentId, new TagInfo
                {
                    Id = 0,
                    FieldId = fieldId,
                    ParentId = parentId,
                    Taxis = taxis,
                    Title = tag
                });

                taxis++;
            }

            return fieldInfo;
        }

        public static FieldInfo Update(IRequest request)
        {
            var siteId = request.GetQueryInt("siteId");
            if (siteId == 0)
            {
                throw new Exception("参数不正确：siteId");
            }

            if (!request.IsAdminLoggin)
            {
                throw new Exception("未授权请求");
            }

            var fieldInfo = request.GetPostObject<FieldInfo>();

            FieldDao.Update(siteId, fieldInfo);

            var fieldId = fieldInfo.Id;
            var parentId = 0;

            var tagInfoList = TagDao.GetTagInfoList(fieldId, parentId);
            if (fieldInfo.Tags == null)
            {
                fieldInfo.Tags = new List<string>();
            }

            var tagInfoListToDelete = new List<TagInfo>();
            foreach (var tagInfo in tagInfoList)
            {
                if (!fieldInfo.Tags.Contains(tagInfo.Title))
                {
                    tagInfoListToDelete.Add(tagInfo);
                }
            }

            if (tagInfoListToDelete.Count > 0)
            {
                TagDao.Delete(fieldId, parentId, tagInfoListToDelete);
            }

            var taxis = 1;
            foreach (var tag in fieldInfo.Tags)
            {
                var tagInfo = tagInfoList.Find(t => t.Title == tag);
                if (tagInfo == null)
                {
                    TagDao.Insert(fieldId, parentId, new TagInfo
                    {
                        Id = 0,
                        FieldId = fieldId,
                        ParentId = parentId,
                        Taxis = taxis,
                        Title = tag
                    });
                }
                else
                {
                    tagInfo.Taxis = taxis;
                    TagDao.Update(fieldId, parentId, tagInfo);
                }

                taxis++;
            }

            return fieldInfo;
        }

        public static bool Delete(IRequest request, string fieldId)
        {
            var siteId = request.GetQueryInt("siteId");
            if (siteId == 0)
            {
                throw new Exception("参数不正确：siteId");
            }

            if (!request.IsAdminLoggin)
            {
                throw new Exception("未授权请求");
            }

            return FieldDao.Delete(siteId, Utils.ToInt(fieldId));
        }
    }
}
