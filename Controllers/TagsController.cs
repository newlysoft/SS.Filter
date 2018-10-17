﻿using System;
using System.Collections.Generic;
using System.Linq;
using SiteServer.Plugin;
using SS.Filter.Core;
using SS.Filter.Model;
using SS.Filter.Provider;

namespace SS.Filter.Controllers
{
    public static class TagsController
    {
        public const string Name = "tags";

        public static TagInfo Get(IRequest request, string tagId)
        {
            var fieldId = request.GetQueryInt("fieldId");
            if (fieldId == 0)
            {
                throw new Exception("参数不正确：fieldId");
            }

            var tagInfo = TagDao.GetTagInfo(Utils.ToInt(tagId));
            if (tagInfo != null)
            {
                tagInfo.TagInfoList = TagDao.GetTagInfoList(fieldId, tagInfo.Id);
                tagInfo.Tags = tagInfo.TagInfoList.Select(x => x.Title).ToList();
            }

            return tagInfo;
        }

        public static TagInfo Update(IRequest request, string tagId)
        {
            if (!request.IsAdminLoggin)
            {
                throw new Exception("未授权请求");
            }

            var tagInfoToUpdate = request.GetPostObject<TagInfo>();

            if (tagInfoToUpdate == null)
            {
                throw new Exception("参数不正确");
            }

            var fieldId = tagInfoToUpdate.FieldId;
            var parentId = tagInfoToUpdate.Id;

            tagInfoToUpdate.Id = Utils.ToInt(tagId);
            TagDao.Update(tagInfoToUpdate.FieldId, tagInfoToUpdate.ParentId, tagInfoToUpdate);

            var tagInfoList = TagDao.GetTagInfoList(fieldId, parentId);
            if (tagInfoToUpdate.Tags == null)
            {
                tagInfoToUpdate.Tags = new List<string>();
            }

            var tagInfoListToDelete = new List<TagInfo>();
            foreach (var tagInfo in tagInfoList)
            {
                if (!tagInfoToUpdate.Tags.Contains(tagInfo.Title))
                {
                    tagInfoListToDelete.Add(tagInfo);
                }
            }

            if (tagInfoListToDelete.Count > 0)
            {
                TagDao.Delete(fieldId, parentId, tagInfoListToDelete);
            }

            var taxis = 1;
            foreach (var tag in tagInfoToUpdate.Tags)
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

            return tagInfoToUpdate;
        }
    }
}
