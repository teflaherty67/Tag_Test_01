#region Namespaces
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

#endregion

namespace Tag_Test_01
{
    [Transaction(TransactionMode.Manual)]
    public class Command : IExternalCommand
    {
        public Result Execute(
          ExternalCommandData commandData,
          ref string message,
          ElementSet elements)
        {
            UIApplication uiapp = commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Application app = uiapp.Application;
            Document doc = uidoc.Document;

            // Get the loaded tag family
            FilteredElementCollector doorsCollector = new FilteredElementCollector(doc)
                .OfClass(typeof(FamilySymbol))
                .OfCategory(BuiltInCategory.OST_Doors)
                .WhereElementIsElementType();

            // Retrieve untagged doors
            FilteredElementCollector untaggedDoorsCollector = new FilteredElementCollector(doc);
            untaggedDoorsCollector.OfCategory(BuiltInCategory.OST_Doors)
                .WhereElementIsNotElementType()
                .WhereElementIsNotElementType()
                .Where(d => !ElementHasTag(doc, d));

            FamilySymbol tagSymbol = GetTagFamilySymbol(doc); // Replace this with your own implementation
            //List<FamilySymbol> tagSymbols = GetTagFamilySymbolList(doc); // This method could be use if you need to get a list of door tags loaded in the family.
            // You would have to list create a form or a gridview to allow the user to select the tag they want to use.
            // tagSymbol = Returned selection from form/gridview


            // Modify document within a transaction
            using (Transaction tx = new Transaction(doc))
            {
                tx.Start("Tag Untagged Doors");

                List<ElementId> tagIds = new List<ElementId>();
                // Create a tag for each untagged door
                foreach (Element door in untaggedDoorsCollector)
                {
                    XYZ tagPosition = GetTagPosition(door); // Replace this with your own implementation

                    Reference doorReference = new Reference(door);

                    // Create the tag
                    IndependentTag newDoorTag = IndependentTag.Create(
                        doc,
                        tagSymbol.Id,
                        doc.ActiveView.Id,
                        doorReference,
                        false,
                        TagOrientation.Horizontal,
                        tagPosition
                    );

                    // Associate the tag with the door
                    newDoorTag.ChangeTypeId(tagSymbol.Id);
                    tagIds.Add(newDoorTag.Id);

                }

                tx.Commit();
            }

            return Result.Succeeded;
        }

        private bool ElementHasTag(Document doc, Element element)
        {
            // Check if the element already has a tag associated with it
            // You can implement your own logic to determine this based on your project requirements
            // Return true if the element has a tag, false otherwise


            var door = doc.GetElement(element.Id);

            // Filter out the already tagged rooms
            if (door != null && door.GetParameters("Tag").FirstOrDefault() == null)
            {
                return true;
            }
            else
                return false;
        }


        private FamilySymbol GetTagFamilySymbol(Document doc)
        {
            // Retrieve the desired tag family symbol to use for tagging
            // You can implement your own logic to get the appropriate symbol based on your project requirements
            // Return the FamilySymbol instance

            // Get the loaded tag family
            FilteredElementCollector tagCollector = new FilteredElementCollector(doc)
                .OfClass(typeof(FamilySymbol))
                .OfCategory(BuiltInCategory.OST_DoorTags)
                .WhereElementIsElementType();
            return tagCollector.Cast<FamilySymbol>().FirstOrDefault();
        }
        private List<FamilySymbol> GetTagFamilySymbolList(Document doc)
        {
            // Retrieve the desired tag family symbol to use for tagging
            // You can implement your own logic to get the appropriate symbol based on your project requirements
            // Return the FamilySymbol instances list

            // Get the loaded tag family
            FilteredElementCollector tagCollector = new FilteredElementCollector(doc)
                .OfClass(typeof(FamilySymbol))
                .OfCategory(BuiltInCategory.OST_DoorTags)
                .WhereElementIsElementType();
            return tagCollector.Cast<FamilySymbol>().ToList();
        }

        private XYZ GetTagPosition(Element element)
        {
            // Determine the desired tag position relative to the element
            // You can implement your own logic to calculate the tag position based on your project requirements
            // Return the XYZ position

            // Create a reference for the Door
            Reference doorReference = new Reference(element);
            // Get the room location point
            XYZ doorLocation = (element.Location as LocationPoint)?.Point;
            return doorLocation;
        }

    }

}
