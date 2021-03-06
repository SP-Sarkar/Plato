﻿using System;
using System.Data;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using PlatoCore.Abstractions;
using PlatoCore.Abstractions.Extensions;
using PlatoCore.Models.Users;

namespace Plato.Entities.Models
{

    public class Entity : SimpleEntity, IEntity
    {

        private readonly ConcurrentDictionary<Type, ISerializable> _metaData;

        public string Message { get; set; }

        public string Html { get; set; }

        public string Abstract { get; set; }

        public string Urls { get; set; }
        
        public int TotalViews { get; set; }

        public int TotalReplies { get; set; }

        public int TotalAnswers { get; set; }

        public int TotalParticipants { get; set; }

        public int TotalReactions { get; set; }

        public int TotalFollows { get; set; }

        public int TotalReports { get; set; }

        public int TotalStars { get; set; }

        public int TotalRatings { get; set; }

        public int SummedRating { get; set; }

        public int MeanRating { get; set; }

        public int TotalLinks { get; set; }

        public int TotalImages { get; set; }

        public int TotalWords { get; set; }
        
        public string IpV4Address { get; set; }

        public string IpV6Address { get; set; }      

        public DateTimeOffset? CreatedDate { get; set; }

        public int EditedUserId { get; set; }

        public DateTimeOffset? EditedDate { get; set; }
   
        public DateTimeOffset? ModifiedDate { get; set; }

        public int LastReplyId { get; set; }

        public int LastReplyUserId { get; set; }

        public DateTimeOffset? LastReplyDate { get; set; }

        public SimpleUser CreatedBy { get; private set; } = new SimpleUser();

        public SimpleUser ModifiedBy { get; private set; } = new SimpleUser();

        public SimpleUser LastReplyBy { get; private set; } = new SimpleUser();

        // IMetaData

        public IEnumerable<IEntityData> Data { get; set; } = new List<EntityData>();

        public IDictionary<Type, ISerializable> MetaData => _metaData;

        public void AddOrUpdate<T>(T obj) where T : class
        {
            if (_metaData.ContainsKey(typeof(T)))
            {
                _metaData.TryUpdate(typeof(T), (ISerializable)obj, _metaData[typeof(T)]);
            }
            else
            {
                _metaData.TryAdd(typeof(T), (ISerializable)obj);
            }
        }

        public void AddOrUpdate(Type type, ISerializable obj)
        {
            if (_metaData.ContainsKey(type))
            {
                _metaData.TryUpdate(type, (ISerializable)obj, _metaData[type]);
            }
            else
            {
                _metaData.TryAdd(type, obj);
            }
        }

        public T GetOrCreate<T>() where T : class
        {
            if (_metaData.ContainsKey(typeof(T)))
            {
                return (T)_metaData[typeof(T)];
            }

            return ActivateInstanceOf<T>.Instance(); 

        }

        public Entity()
        {
            _metaData = new ConcurrentDictionary<Type, ISerializable>();
        }

        // TODO: Move to extension method
        public async Task<EntityUris> GetEntityUrlsAsync()
        {
            if (!string.IsNullOrEmpty(Urls))
            {
                return await Urls.DeserializeAsync<EntityUris>();
            }

            return new EntityUris();
        }

        // IDbModel

        public override void PopulateModel(IDataReader dr)
        {

            base.PopulateModel(dr);

            if (dr.ColumnIsNotNull("Message"))
                Message = Convert.ToString(dr["Message"]);

            if (dr.ColumnIsNotNull("Html"))
                Html = Convert.ToString(dr["Html"]);

            if (dr.ColumnIsNotNull("Abstract"))
                Abstract = Convert.ToString(dr["Abstract"]);

            if (dr.ColumnIsNotNull("Urls")) 
                Urls = Convert.ToString(dr["Urls"]);
            
            if (dr.ColumnIsNotNull("TotalViews"))
                TotalViews = Convert.ToInt32(dr["TotalViews"]);

            if (dr.ColumnIsNotNull("TotalReplies"))
                TotalReplies = Convert.ToInt32(dr["TotalReplies"]);
            
            if (dr.ColumnIsNotNull("TotalAnswers"))
                TotalAnswers = Convert.ToInt32(dr["TotalAnswers"]);

            if (dr.ColumnIsNotNull("TotalParticipants"))
                TotalParticipants = Convert.ToInt32(dr["TotalParticipants"]);

            if (dr.ColumnIsNotNull("TotalReactions"))
                TotalReactions = Convert.ToInt32(dr["TotalReactions"]);
            
            if (dr.ColumnIsNotNull("TotalFollows"))
                TotalFollows = Convert.ToInt32(dr["TotalFollows"]);

            if (dr.ColumnIsNotNull("TotalReports"))
                TotalReports = Convert.ToInt32(dr["TotalReports"]);

            if (dr.ColumnIsNotNull("TotalStars"))
                TotalStars = Convert.ToInt32(dr["TotalStars"]);

            if (dr.ColumnIsNotNull("TotalRatings"))
                TotalRatings = Convert.ToInt32(dr["TotalRatings"]);

            if (dr.ColumnIsNotNull("SummedRating"))
                SummedRating = Convert.ToInt32(dr["SummedRating"]);

            if (dr.ColumnIsNotNull("MeanRating"))
                MeanRating = Convert.ToInt32(dr["MeanRating"]);
            
            if (dr.ColumnIsNotNull("TotalLinks"))
                TotalLinks = Convert.ToInt32(dr["TotalLinks"]);
            
            if (dr.ColumnIsNotNull("TotalImages"))
                TotalImages = Convert.ToInt32(dr["TotalImages"]);
            
            if (dr.ColumnIsNotNull("TotalWords"))
                TotalWords = Convert.ToInt32(dr["TotalWords"]);            

            if (dr.ColumnIsNotNull("IpV4Address"))
                IpV4Address = Convert.ToString(dr["IpV4Address"]);

            if (dr.ColumnIsNotNull("IpV6Address"))
                IpV6Address = Convert.ToString(dr["IpV6Address"]);

            if (CreatedUserId > 0)
            {
                CreatedBy.Id = CreatedUserId;
                if (dr.ColumnIsNotNull("CreatedUserName"))
                    CreatedBy.UserName = Convert.ToString(dr["CreatedUserName"]);
                if (dr.ColumnIsNotNull("CreatedDisplayName"))
                    CreatedBy.DisplayName = Convert.ToString(dr["CreatedDisplayName"]);
                if (dr.ColumnIsNotNull("CreatedAlias"))
                    CreatedBy.Alias = Convert.ToString(dr["CreatedAlias"]);
                if (dr.ColumnIsNotNull("CreatedPhotoUrl"))
                    CreatedBy.PhotoUrl = Convert.ToString(dr["CreatedPhotoUrl"]);
                if (dr.ColumnIsNotNull("CreatedPhotoColor"))
                    CreatedBy.PhotoColor = Convert.ToString(dr["CreatedPhotoColor"]);
                if (dr.ColumnIsNotNull("CreatedSignatureHtml"))
                    CreatedBy.SignatureHtml = Convert.ToString(dr["CreatedSignatureHtml"]);
                if (dr.ColumnIsNotNull("CreatedIsVerified"))
                    CreatedBy.IsVerified = Convert.ToBoolean(dr["CreatedIsVerified"]);
                if (dr.ColumnIsNotNull("CreatedIsStaff"))
                    CreatedBy.IsStaff = Convert.ToBoolean(dr["CreatedIsStaff"]);
                if (dr.ColumnIsNotNull("CreatedIsSpam"))
                    CreatedBy.IsSpam = Convert.ToBoolean(dr["CreatedIsSpam"]);
                if (dr.ColumnIsNotNull("CreatedIsBanned"))
                    CreatedBy.IsBanned = Convert.ToBoolean(dr["CreatedIsBanned"]);
            }

            if (dr.ColumnIsNotNull("CreatedDate"))
                CreatedDate = (DateTimeOffset)dr["CreatedDate"];
            
            if (dr.ColumnIsNotNull("EditedUserId"))
                EditedUserId = Convert.ToInt32(dr["EditedUserId"]);
            
            if (dr.ColumnIsNotNull("EditedDate"))
                EditedDate = (DateTimeOffset)dr["EditedDate"];

            if (ModifiedUserId > 0)
            {
                ModifiedBy.Id = ModifiedUserId;
                if (dr.ColumnIsNotNull("ModifiedUserName"))
                    ModifiedBy.UserName = Convert.ToString(dr["ModifiedUserName"]);
                if (dr.ColumnIsNotNull("ModifiedDisplayName"))
                    ModifiedBy.DisplayName = Convert.ToString(dr["ModifiedDisplayName"]);
                if (dr.ColumnIsNotNull("ModifiedAlias"))
                    ModifiedBy.Alias = Convert.ToString(dr["ModifiedAlias"]);
                if (dr.ColumnIsNotNull("ModifiedPhotoUrl"))
                    ModifiedBy.PhotoUrl = Convert.ToString(dr["ModifiedPhotoUrl"]);
                if (dr.ColumnIsNotNull("ModifiedPhotoColor"))
                    ModifiedBy.PhotoColor = Convert.ToString(dr["ModifiedPhotoColor"]);
                if (dr.ColumnIsNotNull("ModifiedSignatureHtml"))
                    ModifiedBy.SignatureHtml = Convert.ToString(dr["ModifiedSignatureHtml"]);
                if (dr.ColumnIsNotNull("ModifiedIsVerified"))
                    ModifiedBy.IsVerified = Convert.ToBoolean(dr["ModifiedIsVerified"]);
                if (dr.ColumnIsNotNull("ModifiedIsStaff"))
                    ModifiedBy.IsStaff = Convert.ToBoolean(dr["ModifiedIsStaff"]);
                if (dr.ColumnIsNotNull("ModifiedIsSpam"))
                    ModifiedBy.IsSpam = Convert.ToBoolean(dr["ModifiedIsSpam"]);
                if (dr.ColumnIsNotNull("ModifiedIsBanned"))
                    ModifiedBy.IsBanned = Convert.ToBoolean(dr["ModifiedIsBanned"]);
            }

            if (dr.ColumnIsNotNull("ModifiedDate"))
                ModifiedDate = (DateTimeOffset)dr["ModifiedDate"];
         
            if (dr.ColumnIsNotNull("LastReplyId"))
                LastReplyId = Convert.ToInt32(dr["LastReplyId"]);

            if (dr.ColumnIsNotNull("LastReplyUserId"))
                LastReplyUserId = Convert.ToInt32(dr["LastReplyUserId"]);

            if (LastReplyUserId > 0)
            {
                LastReplyBy.Id = LastReplyUserId;
                if (dr.ColumnIsNotNull("LastReplyUserName"))
                    LastReplyBy.UserName = Convert.ToString(dr["LastReplyUserName"]);
                if (dr.ColumnIsNotNull("LastReplyDisplayName"))
                    LastReplyBy.DisplayName = Convert.ToString(dr["LastReplyDisplayName"]);
                if (dr.ColumnIsNotNull("LastReplyPhotoUrl"))
                    LastReplyBy.PhotoUrl = Convert.ToString(dr["LastReplyPhotoUrl"]);
                if (dr.ColumnIsNotNull("LastReplyPhotoColor"))
                    LastReplyBy.PhotoColor = Convert.ToString(dr["LastReplyPhotoColor"]);
                if (dr.ColumnIsNotNull("LastReplySignatureHtml"))
                    LastReplyBy.SignatureHtml = Convert.ToString(dr["LastReplySignatureHtml"]);
                if (dr.ColumnIsNotNull("LastReplyIsVerified"))
                    LastReplyBy.IsVerified = Convert.ToBoolean(dr["LastReplyIsVerified"]);
                if (dr.ColumnIsNotNull("LastReplyIsStaff"))
                    LastReplyBy.IsStaff = Convert.ToBoolean(dr["LastReplyIsStaff"]);
                if (dr.ColumnIsNotNull("LastReplyIsSpam"))
                    LastReplyBy.IsSpam = Convert.ToBoolean(dr["LastReplyIsSpam"]);
                if (dr.ColumnIsNotNull("LastReplyIsBanned"))
                    LastReplyBy.IsBanned = Convert.ToBoolean(dr["LastReplyIsBanned"]);
            }
            
            if (dr.ColumnIsNotNull("LastReplyDate"))
                LastReplyDate = (DateTimeOffset)dr["LastReplyDate"];

        }

    }

}
