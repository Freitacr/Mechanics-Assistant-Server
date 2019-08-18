using System.Runtime.Serialization;

namespace MechanicsAssistantServer.Data
{
    [DataContract]
    public class MechanicQuery
    {
        [DataMember(Name = "make")]
        public string Make { get; set; }

        [DataMember(Name = "model")]
        public string Model { get; set; }

        [DataMember(Name = "problem", IsRequired = false)]
        public string Problem { get; set; }

        [DataMember(Name = "complaint")]
        public string Complaint { get; set; }

        [DataMember(Name = "vin", IsRequired = false)]
        public string Vin { get; set; }

        public MechanicQuery(string make, string model, string problem, string vin, string complaint)
        {
            Make = make;
            Model = model;
            Problem = problem;
            Vin = vin;
            Complaint = complaint;
        }

        // override object.Equals
        public override bool Equals(object obj)
        {
            //       
            // See the full list of guidelines at
            //   http://go.microsoft.com/fwlink/?LinkID=85237  
            // and also the guidance for operator== at
            //   http://go.microsoft.com/fwlink/?LinkId=85238
            //

            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }

            MechanicQuery other = obj as MechanicQuery;
            if (Make != other.Make)
                return false;
            if (Model != other.Model)
                return false;
            if (Vin != other.Vin)
                return false;
            if (Complaint != other.Complaint)
                return false;
            return Problem == other.Problem;
        }

        // override object.GetHashCode
        public override int GetHashCode()
        {
            int ret = (Make ?? "").GetHashCode();
            ret += (Model ?? "").GetHashCode();
            ret += (Vin ?? "").GetHashCode();
            ret += (Complaint ?? "").GetHashCode();
            ret += (Problem ?? "").GetHashCode();
            return ret;
            // TODO: write your implementation of GetHashCode() here


        }
    }
}
