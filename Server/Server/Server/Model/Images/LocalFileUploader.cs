﻿
namespace Server.Model.Images
{
    /// <summary>
    /// Classe responsable du téléchargement et du renommage de fichiers en local.
    /// </summary>
    public class LocalFileUploader : IFileUploader
    {
        private string profilePicsPath;

        public LocalFileUploader(string profilePicsPath)
        {
            this.profilePicsPath = profilePicsPath;
        }

        /// <summary>
        /// Télécharge une photo de profil et la sauvegarde dans le répertoire local.
        /// </summary>
        /// <param name="file">Le fichier à télécharger.</param>
        /// <param name="fileName">Le nom du fichier.</param>
        public void UploadProfilePic(IFormFile file, string fileName)
        {
            try
            {
                // Détermine le chemin complet du fichier
                string filePath = Path.Combine(profilePicsPath, fileName + Path.GetExtension(file.FileName));

                // Sauvegarde le fichier sur le disque local
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    file.CopyTo(fileStream);
                }
            }
            catch (System.Exception ex)
            {
                throw new System.Exception($"Erreur lors de l'upload de la photo de profil : {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Renomme une photo de profil dans le répertoire local.
        /// </summary>
        /// <param name="oldFileName">L'ancien nom du fichier.</param>
        /// <param name="newFileName">Le nouveau nom du fichier.</param>
        public void RenameProfilePic(string oldFileName, string newFileName)
        {
            try
            {
                // Cherche l'extension du fichier à renommer
                var oldFilePath = Directory.GetFiles(profilePicsPath, oldFileName + ".*").FirstOrDefault();

                // si le fichier n'existe pas cela veut dire que l'utilisateur n'a pas de photo de profil, il n'y a donc rien à renommer
                if (oldFilePath == null) {

                }
                else { // sinon on le renomme
                    // Détermine le nouveau chemin avec l'extension actuelle
                    string newFilePath = Path.Combine(profilePicsPath, newFileName + Path.GetExtension(oldFilePath));

                    // Renomme le fichier
                    File.Move(oldFilePath, newFilePath);
                }

            }
            catch (System.Exception ex)
            {
                throw new System.Exception($"Erreur lors du renommage de la photo de profil : {ex.Message}", ex);
            }
        }

        /// <summary>
        /// récupère une photo de profil dans le répertoire local.
        /// </summary>
        /// <param name="fileName">Le nom du fichier</param>
        /// <returns>La photo de profil en bytes ou l'image de profil par défaut</returns>
        public byte[] GetProfilePic(string fileName)
        {
            byte[] result = null;
            string filePath = Directory.GetFiles(profilePicsPath, $"{fileName}.*").FirstOrDefault();

            if (filePath != null)
            {
                result = File.ReadAllBytes(filePath);
            }
            else
            {
                result = File.ReadAllBytes(Path.Combine(profilePicsPath, "default.png"));
            }
            return result;
        }
    }
}
