﻿using FluentFTP;
using Microsoft.Extensions.Options;
using Server.Model.Images;

namespace Server.Model.Managers
{
    /// <summary>  
    /// Gère l'upload et la modification d'images  
    /// </summary>  
    public class ImageManager
    {
        private readonly IFileUploader fileUploader;

        public ImageManager(IFileUploader fileUploader)
        {
            this.fileUploader = fileUploader;
        }


        /// <summary>  
        /// Télécharge une photo de profil.  
        /// </summary>  
        /// <param name="file">Le fichier de la photo de profil.</param>  
        /// <param name="username">Le nom d'utilisateur associé à la photo de profil.</param>  
        public void UploadProfilePic(IFormFile file, string username)
        {
            fileUploader.UploadProfilePic(file, username);
        }

        /// <summary>
        /// Renomme une photo de profil par le nouveau nom d'utilisateur
        /// </summary>
        /// <param name="oldUsername">L'ancien nom d'utilisateur</param>
        /// <param name="newUsername">Le nouveau nom d'utilisateur</param>
        public void RenameProfilePic(string oldUsername, string newUsername)
        {
            fileUploader.RenameProfilePic(oldUsername, newUsername);
        }

        /// <summary>
        /// Renvoie la photo de profil d'un utilisateur donné
        /// </summary>
        /// <param name="username">Le nom d'utilisateur dont on souhaite la photo</param>
        /// <returns>La photo de profil associé à cet utilisateur sous forme de tableau de bytes</returns>
        public byte[] GetProfilePic(string username)
        {
            return fileUploader.GetProfilePic(username);
        }
    }
}