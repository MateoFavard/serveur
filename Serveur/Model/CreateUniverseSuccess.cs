namespace Serveur.Model
{
    public class CreateUniverseSuccess : CreateUniverse
    {
        private String _reason;

        public CreateUniverseSuccess(String reason) : base(true)
        {
            this._reason = reason;
        }

        public String Reason => this._reason;
    }
}
