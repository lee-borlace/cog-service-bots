using Microsoft.ProjectOxford.Face.Contract;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Watcher.Model
{
    /// <summary>
    /// Describes the difference between two observations.
    /// </summary>
    public class ObservationDelta
    {
        public List<string> IdentifiedPeopleAdded { get; set; }
        public List<string> IdentifiedPeopleRemoved { get; set; }
        public List<string> UnidentifiedPeopleAdded { get; set; }
        public List<string> UnidentifiedPeopleRemoved { get; set; }
        public List<string> ObjectsAdded { get; set; }
        public List<string> ObjectsRemoved { get; set; }

        public static ObservationDelta CalculateDelta(Observation previousObservation, Observation currentObservation, Dictionary<Guid, Person> personMappings)
        {
            var identifiedPeoplePrevious = previousObservation != null ? previousObservation.GetIdentifiedPeopleList(personMappings) : new List<string>();
            var identifiedPeopleCurrent = currentObservation != null ? currentObservation.GetIdentifiedPeopleList(personMappings) : new List<string>();

            var unidentifiedPeoplePrevious = previousObservation != null ? previousObservation.GetUnidentifiedPeopleDescriptionList() : new List<string>();
            var unidentifiedPeopleCurrent = currentObservation != null ? currentObservation.GetUnidentifiedPeopleDescriptionList() : new List<string>();

            var objectsPrevious = previousObservation != null ? previousObservation.GetObjectsList() : new List<string>();
            var objectsCurrent = currentObservation != null ? currentObservation.GetObjectsList() : new List<string>();

            return new ObservationDelta()
            {
                IdentifiedPeopleAdded = identifiedPeopleCurrent.Where(p => !identifiedPeoplePrevious.Contains(p)).ToList(),
                IdentifiedPeopleRemoved = identifiedPeoplePrevious.Where(p => !identifiedPeopleCurrent.Contains(p)).ToList(),

                UnidentifiedPeopleAdded = unidentifiedPeopleCurrent.Where(p => !unidentifiedPeoplePrevious.Contains(p)).ToList(),
                UnidentifiedPeopleRemoved = unidentifiedPeoplePrevious.Where(p => !unidentifiedPeopleCurrent.Contains(p)).ToList(),

                ObjectsAdded = objectsCurrent.Where(p => !objectsPrevious.Contains(p)).ToList(),
                ObjectsRemoved = objectsPrevious.Where(p => !objectsCurrent.Contains(p)).ToList()
            };
        }
    }
}
