using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace R_4_23
{
    [Transaction(TransactionMode.Manual)]
    public class Main : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            try
            { 
            // Обработали документ
            UIDocument uiDoc = commandData.Application.ActiveUIDocument;
            Document doc = uiDoc.Document;

            // Попросили пользователя выбрать группу
            GroupPickFilter groupPickFilter = new GroupPickFilter();
            Reference reference = uiDoc.Selection.PickObject(Autodesk.Revit.UI.Selection.ObjectType.Element, groupPickFilter, "Выберите группу объектов");
            Element element = doc.GetElement(reference);
            Group group = element as Group;               

            // Находим центр группы
            XYZ groupCenter = GetElementCenter(group);
            
            // Находим комнату в которой была группа и ее центр
            Room room = GetRoomByPoint(doc, groupCenter);
            XYZ roomCenter = GetElementCenter(room);

            // Определяем смещение центра группы относительно центра комнаты
            XYZ offset = groupCenter - roomCenter;


                /// Пользователь выбирает точку пикая где-то внутри комнаты
                /// Воспользуемся методом GetRoomByPoint чтобы определить комнату по которой пикнул пользователь
                /// Нйти ее центр
                /// На основе смещения offset вычислить точку в которую необходимо реализовать вставку группы


                // Код отвечающий за реализацию функционала копирования
                XYZ point = uiDoc.Selection.PickPoint("Выберите точку");

                Room roomPick = GetRoomByPoint(doc, point);
                XYZ roomPickCenter = GetElementCenter(roomPick);
                XYZ offsetPick = offset + roomPickCenter;


                Transaction transaction = new Transaction(doc);
            transaction.Start("Копирование группы объектов");
            doc.Create.PlaceGroup(offsetPick, group.GroupType);
            transaction.Commit();
            }
            // Отработка исключения на нажатие Esc
            catch(Autodesk.Revit.Exceptions.OperationCanceledException)
            { 
                return Result.Cancelled; 
            }
            // Отработка исключения на любые другие ошибки
            catch (Exception ex)
            {
                message = ex.Message;
            }
            return Result.Succeeded;
        }
        
        // Метод для поиска центра элемента
        public XYZ GetElementCenter(Element element)
        {
            BoundingBoxXYZ bounding = element.get_BoundingBox(null);
            return (bounding.Max + bounding.Min) / 2;
        }

        // Метод для определения комнаты в которой расположена выбранная группа
        public Room GetRoomByPoint(Document doc, XYZ point)
        {
            FilteredElementCollector collector = new FilteredElementCollector(doc);
            collector.OfCategory(BuiltInCategory.OST_Rooms);
            foreach (Element e in collector)
            {
                Room room = e as Room;
                if(room!=null)
                {
                    if(room.IsPointInRoom(point))
                    {
                        return room;
                    }
                }
            }
            return null;
        }
    }
   
    // Класс определяющий является ли элемент группой
    public class GroupPickFilter : ISelectionFilter
    {
        public bool AllowElement(Element elem)
        {
            if (elem.Category.Id.IntegerValue == (int)BuiltInCategory.OST_IOSModelGroups)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool AllowReference(Reference reference, XYZ position)
        {
            return false;
        }
    }
}

