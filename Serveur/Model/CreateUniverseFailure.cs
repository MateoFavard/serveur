namespace Serveur.Model
{
    public class CreateUniverseFailure: CreateUniverse
    {
        private String _reason;

        public CreateUniverseFailure(String reason) : base(false)
        {
            this._reason = reason;
        }

        public String Reason => this._reason;
    }
}
