namespace Serveur.Model
{
    public class CreateUniverse
    {
        private bool _success;

        public CreateUniverse(bool success)
        {
            this._success = success;
        }

        public bool Success => this._success;
    }
}
