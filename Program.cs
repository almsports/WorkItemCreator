using Microsoft.TeamFoundation.Client;
using Microsoft.TeamFoundation.TestManagement.Client;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
using NLipsum.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestApp
{
    class Program
    {
        public static int Count { get; set; }
        static void Main(string[] args)
        {
            Console.WriteLine("Enter TFS Server Url: ");
            var serverUrl = Console.ReadLine();

            TfsTeamProjectCollection tfsCollection = new TfsTeamProjectCollection(new Uri(serverUrl));
            tfsCollection.EnsureAuthenticated();

            WorkItemStore store = (WorkItemStore)tfsCollection.GetService(typeof(WorkItemStore));

            Console.WriteLine("Enter TFS Project Name: ");
            var projectName = Console.ReadLine();

            Project project = store.Projects[projectName];

            Console.WriteLine("How many Feature do you want to create?: ");
            var featureCount = Convert.ToInt32(Console.ReadLine());
            Console.WriteLine("How many PBIs do you want to create for every feature Item?: ");
            var pbiCount = Convert.ToInt32(Console.ReadLine());
            Console.WriteLine("How many Tasks do you want to create for every PBI Item?: ");
            var taskCount = Convert.ToInt32(Console.ReadLine());
            Console.WriteLine("How many Bugs do you want to create for every PBI Item?: ");
            var bugCount = Convert.ToInt32(Console.ReadLine());
            Console.WriteLine("How many TestCases do you want to create for every PBI Item?: ");
            var testCount = Convert.ToInt32(Console.ReadLine());

            try
            {            
                for (int i = 0; i < featureCount; i++)
                {
                    Console.WriteLine("Creating new Feature Work Item ... ");
                    WorkItem workItem = CreateWorkItem(project, "Feature");
                    workItem.Save();

                    for (int j = 0; j < pbiCount; j++)
                    {
                        Console.WriteLine("Creating new Product Backlog Item ... ");
                        WorkItem subItem = CreateWorkItem(project, "Product Backlog Item");
                        SetWorkItemState(subItem, j);
                        WorkItemLinkType hierarchyLinkType = CreateParentChildRelation(store, subItem, workItem.Id);
                        subItem.Save();

                        Console.WriteLine("Creating new Tasks ... ");
                        for (int k = 0; k < taskCount; k++)
                        {                            
                            WorkItem subsubItem = CreateWorkItem(project, "Task");
                            SetWorkItemState(subsubItem, j);                            
                            WorkItemLinkType hierarchyLinkType2 = CreateParentChildRelation(store, subsubItem, subItem.Id);
                            subsubItem.Save();
                        }

                        Console.WriteLine("Creating new Bugs ... ");
                        for (int l = 0; l < bugCount; l++)
                        {
                            WorkItem subBugItem = CreateWorkItem(project, "Bug");
                            SetWorkItemState(subBugItem, j);                            
                            WorkItemLinkType hierarchyLinkType3 = CreateParentChildRelation(store, subBugItem, workItem.Id);
                            CreateSimpleRelation(store, subBugItem, subItem.Id);
                            subBugItem.Save();
                        }

                        Console.WriteLine("Creating new Test Cases ... ");
                        for (int k = 0; k < testCount; k++)
                        {
                            WorkItem subTestItem = CreateWorkItem(project, "Test Case");
                            SetWorkItemState(subTestItem, j);                            
                            CreateTestRelation(store, subTestItem, subItem.Id); 
                            subTestItem.Save();
                        }
                    }
                
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine( "********Cannot Save Work Item Relation*********");
                Console.WriteLine(ex.Message);
            }

            Console.WriteLine("Es wurden {0} Work Items erstellt.", Count.ToString());
            Console.WriteLine("Beenden Sie das Programm mit einer beliebigen Taste ...");
            Console.ReadLine();
        }

        
        private static void SetWorkItemState(WorkItem item, int j)
        {
            Random rnd = new Random(j);
            var no = rnd.Next(0, 6);
                        
            if (no /2 >= 1 && no / 2 < 2)
            {
                if (item.Type.Name == "Product Backlog Item" || item.Type.Name == "Bug")
                    item.State = "Approved";
                else if (item.Type.Name == "Task" || item.Type.Name == "Feature")
                    item.State = "In Progress";
                else if (item.Type.Name == "Test Case")
                    item.State = "Ready";
            }
            else if (no / 2 >= 2)
            {
                if (item.Type.Name == "Product Backlog Item" || item.Type.Name == "Bug")
                    item.State = "Committed";
            }
            else if (no / 2 >= 3)
            {
                if (item.Type.Name == "Product Backlog Item" || item.Type.Name == "Bug" || item.Type.Name == "Task")
                    item.State = "Done";
                else if (item.Type.Name == "Test Case")
                    item.State = "Closed";
            }

            //item.Save();
        }

        private static void CreateSimpleRelation(WorkItemStore store, WorkItem subItem, int relationId)
        {
            WorkItemLinkType hlt = store.WorkItemLinkTypes[CoreLinkTypeReferenceNames.Related];
            subItem.Links.Add(new WorkItemLink(hlt.ReverseEnd, relationId));
            //subItem.Save();
        }

        private static void CreateTestRelation(WorkItemStore store, WorkItem workItem, int relationId)
        {
            WorkItemLinkTypeEnd hierarchyLinkType = store.WorkItemLinkTypes.LinkTypeEnds["Tests"];
            workItem.Links.Add(new WorkItemLink(hierarchyLinkType, relationId));            
            //workItem.Save();
        }

        private static WorkItemLinkType CreateParentChildRelation(WorkItemStore store, WorkItem subItem, int parentId)
        {
            WorkItemLinkType hierarchyLinkType = store.WorkItemLinkTypes[CoreLinkTypeReferenceNames.Hierarchy];
            subItem.Links.Add(new WorkItemLink(hierarchyLinkType.ReverseEnd, parentId));
//            subItem.Save();
            return hierarchyLinkType;
        }

        private static WorkItem CreateWorkItem(Project project, string workItemType)
        {
            LipsumGenerator gen = new LipsumGenerator();
            Random rnd = new Random();
                        
            WorkItemType wiType = project.WorkItemTypes[workItemType];

            // Create the work item. 
            WorkItem work = new WorkItem(wiType)
            {
                Title =  string.Join(" ", gen.GenerateWords(4)),
                Description =  gen.GenerateLipsum(4, Features.Sentences, "")
            };

            if (workItemType == "Task")
            {
                work.Fields["Remaining Work"].Value = rnd.Next(2, 10);                
            }
            else if (workItemType == "Product Backlog Item" || workItemType == "Bug")
            {
                work.Fields["Effort"].Value = rnd.Next(1, 13);
            }

            // Save the new user story. 
            work.Save();
            Count++;
            return work;
        }
    }
}
