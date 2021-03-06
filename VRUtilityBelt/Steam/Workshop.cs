﻿using Steamworks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VRUB.Utility;

namespace VRUB.Steam
{
    class Workshop
    {
        protected static Callback<ItemInstalled_t> _itemInstalled;
        protected static Callback<RemoteStoragePublishedFileUnsubscribed_t> _fileUnsubscribed;
        protected static Callback<RemoteStoragePublishedFileSubscribed_t> _fileSubscribed;

        public delegate void OnItemInstalled(ItemInstalled_t args);
        public delegate void OnFileSubscribed(RemoteStoragePublishedFileSubscribed_t args);
        public delegate void OnFileUnsubscribed(RemoteStoragePublishedFileUnsubscribed_t args);

        public static event OnItemInstalled ItemInstalled;
        public static event OnFileSubscribed FileSubscribed;
        public static event OnFileUnsubscribed FileUnsubscribed;

        public static void RegisterCallbacks()
        {
            _itemInstalled = Callback<ItemInstalled_t>.Create((callback) =>
            {
                if(callback.m_unAppID == SteamManager.AppID)
                    ItemInstalled?.Invoke(callback);
            });

            _fileUnsubscribed = Callback<RemoteStoragePublishedFileUnsubscribed_t>.Create((callback) =>
            {
                if (callback.m_nAppID == SteamManager.AppID)
                    FileUnsubscribed?.Invoke(callback);
            });

            _fileSubscribed = Callback<RemoteStoragePublishedFileSubscribed_t>.Create((callback) =>
            {
                if (callback.m_nAppID == SteamManager.AppID)
                    FileSubscribed?.Invoke(callback);
            });
        }

        public static Dictionary<PublishedFileId_t, string> GetSubscribedItems() {
            if (!SteamManager.Initialised)
                return null;

            uint numSubbed = SteamUGC.GetNumSubscribedItems();

            PublishedFileId_t[] fileIds = new PublishedFileId_t[numSubbed];

            SteamUGC.GetSubscribedItems(fileIds, numSubbed);

            ulong itemSize = 0;
            uint updatedTimestamp = 0;
            string folderPath = null;

            Dictionary<PublishedFileId_t, string> paths = new Dictionary<PublishedFileId_t, string>();

            if (fileIds != null)
            {
                foreach (PublishedFileId_t fileId in fileIds)
                {
                    EItemState state = (EItemState)SteamUGC.GetItemState(fileId);

                    if ((state & EItemState.k_EItemStateInstalled) != 0) {
                        if (SteamUGC.GetItemInstallInfo(fileId, out itemSize, out folderPath, 1024, out updatedTimestamp))
                            paths.Add(fileId, folderPath);
                    } else {
                        Logger.Debug("[WORKSHOP] State for Workshop Item " + fileId.m_PublishedFileId.ToString() + ": " + state.ToString());
                    }
                }
            }

            return paths;
        }


    }
}
