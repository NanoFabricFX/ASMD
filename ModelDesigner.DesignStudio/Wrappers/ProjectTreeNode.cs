﻿//___________________________________________________________________________________
//
//  Copyright (C) 2020, Mariusz Postol LODZ POLAND.
//
//  To be in touch join the community at GITTER: https://gitter.im/mpostol/OPC-UA-OOI
//___________________________________________________________________________________

using CAS.CommServer.UA.ModelDesigner.Configuration.IO;
using CAS.UA.Model.Designer.IO;
using CAS.UA.Model.Designer.Properties;
using CAS.UA.Model.Designer.Solution;
using CAS.UA.Model.Designer.ToForms;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Xml;
using UAOOI.SemanticData.UANodeSetValidation;
using OPCFModelDesign = Opc.Ua.ModelCompiler.ModelDesign;

namespace CAS.UA.Model.Designer.Wrappers
{
  internal interface IProjectModel : IBaseModel
  {
    string Name { get; }

    void Remove();
  }

  internal class ProjectTreeNode : WrapperTreeNode, IProjectModel
  {
    #region private

    //constants
    private const string m_ModelExtension = ".xml";

    //var
    private static readonly object m_BuildLockObject = new object(); // this object is used to prevent many code generator usage at the same time

    private static UniqueNameGenerator m_UniqueNameGenerator = new UniqueNameGenerator(Resources.DefaultProjectName);
    private readonly ISolutionDirectoryPathManagement m_SolutionHomeDirectory;
    private UAModelDesignerProject b_UAModelDesignerProject;
    private static string m_GetNextUniqueProjectName => m_UniqueNameGenerator.GenerateNewName();

    private void InitializeComponent(ModelDesign model)
    {
      Model = model;
      Add(model);
    }

    private static string ReplaceTokenAndReturnFullPath(string fileNameToBeProcessed, string projectName, string solutionDirectory)
    {
      string _Name = fileNameToBeProcessed.Replace(Resources.Token_ProjectFileName, projectName);
      return RelativeFilePathsCalculator.CalculateAbsoluteFileName(_Name, solutionDirectory);
    }

    #endregion private

    #region constructors

    private ProjectTreeNode(ISolutionDirectoryPathManagement solutionPath, string nodeName) : base(null, nodeName)
    {
      m_SolutionHomeDirectory = solutionPath;
      m_SolutionHomeDirectory.BaseDirectoryPathChanged += SolutionHomeDirectoryBaseDirectoryPathChanged;
    }

    private void SolutionHomeDirectoryBaseDirectoryPathChanged(object sender, NewDirectoryPathEventArgs e)
    {
      throw new NotImplementedException();
    }

    private ProjectTreeNode(ISolutionDirectoryPathManagement solutionPath, string filePath, OPCFModelDesign model) : this(solutionPath, Path.GetFileNameWithoutExtension(filePath))
    {
      UAModelDesignerProject = new UAModelDesignerProject()
      {
        BuildOutputDirectoryName = Resources.DefaultOutputBuildDirectory,
        CSVFileName = Resources.DefaultCSVFileName,
        FileName = RelativeFilePathsCalculator.TryComputeRelativePath(solutionPath.BaseDirectory, filePath),
        ProjectIdentifier = Guid.NewGuid().ToString(),
        Name = m_GetNextUniqueProjectName
      };
      InitializeComponent(new ModelDesign(model, false));
    }

    internal ProjectTreeNode(ISolutionDirectoryPathManagement solutionPath, UAModelDesignerProject projectDescription) : this(solutionPath, projectDescription.Name)
    {
      UAModelDesignerProject = projectDescription;
      ModelDesign _RootOfOPCUAInfromationModel = ModelDesign.CreateRootOfOPCUAInfromationModel(FileName);
      InitializeComponent(_RootOfOPCUAInfromationModel);
    }

    internal static ProjectTreeNode ImportNodeSet(ISolutionDirectoryPathManagement solutionPathProvider, Action<TraceMessage> traceEvent, Func<string, Action<TraceMessage>, Tuple<OPCFModelDesign, string>> importNodeSet)
    {
      Tuple<OPCFModelDesign, string> _model = importNodeSet(solutionPathProvider.BaseDirectory, traceEvent);
      if (_model == null)
        return null;
      return new ProjectTreeNode(solutionPathProvider, _model.Item2, _model.Item1);
    }

    internal static ProjectTreeNode CreateNewModel(ISolutionDirectoryPathManagement solutionPathProvider)
    {
      string _DefaultFileName = Path.Combine(solutionPathProvider.BaseDirectory, m_GetNextUniqueProjectName);
      return new ProjectTreeNode(solutionPathProvider, _DefaultFileName, new OPCFModelDesign());
    }

    #endregion constructors

    #region WrapperTreeNode

    public override object Wrapper => this.Create();
    public override NodeTypeEnum NodeType => NodeTypeEnum.ProjectNode;

    public override Dictionary<FolderType, IEnumerable<IModelNodeAdvance>> GetFolders()
    {
      Dictionary<FolderType, IEnumerable<IModelNodeAdvance>> toBeReturned = base.GetFolders();
      toBeReturned.Add(FolderType.Model, Model);
      return toBeReturned;
    }

    /// <summary>
    /// Gets the name of the help topic.
    /// </summary>
    /// <value>The name of the help topic.</value>
    public override string HelpTopicName => Resources.ProjectTreeNode;

    /// <summary>
    /// Gets the node class.
    /// </summary>
    /// <value>The node class.</value>
    public override NodeClassesEnum NodeClass => NodeClassesEnum.None;

    #endregion WrapperTreeNode

    #region public

    internal UAModelDesignerProject UAModelDesignerProject
    {
      get
      {
        b_UAModelDesignerProject.Name = this.Text;
        return b_UAModelDesignerProject;
      }
      private set
      {
        b_UAModelDesignerProject = value;
        this.Text = b_UAModelDesignerProject.Name;
      }
    }

    /// <summary>
    /// Gets or sets the model file name.
    /// </summary>
    /// <value>The name of the file.</value>
    internal string FileName
    {
      get
      {
        if (!Path.HasExtension(UAModelDesignerProject.FileName))
          return $"{UAModelDesignerProject.FileName}.{m_ModelExtension}";
        else
          return UAModelDesignerProject.FileName;
      }
    }

    /// <summary>
    /// Calculates the effective absolute model file path.
    /// </summary>
    /// <returns>System.String.</returns>
    internal string CalculateEffectiveAbsoluteModelFilePath()
    {
      return RelativeFilePathsCalculator.CalculateAbsoluteFileName(this.FileName, m_SolutionHomeDirectory.BaseDirectory);
    }

    internal string CSVFileName
    {
      get
      {
        if (string.IsNullOrEmpty(UAModelDesignerProject.CSVFileName))
          UAModelDesignerProject.CSVFileName = Resources.DefaultCSVFileName;
        return UAModelDesignerProject.CSVFileName;
      }
      set => UAModelDesignerProject.CSVFileName = value;
    }

    internal string CSVFilePath => ReplaceTokenAndReturnFullPath(CSVFileName, UAModelDesignerProject.Name, m_SolutionHomeDirectory.BaseDirectory);

    internal Guid ProjectIdentifier
    {
      get => new Guid(UAModelDesignerProject.ProjectIdentifier);
      set => UAModelDesignerProject.ProjectIdentifier = value.ToString();
    }

    internal string BuildOutputDirectoryName
    {
      get
      {
        if (string.IsNullOrEmpty(UAModelDesignerProject.BuildOutputDirectoryName))
          UAModelDesignerProject.BuildOutputDirectoryName = Resources.DefaultOutputBuildDirectory;
        return UAModelDesignerProject.BuildOutputDirectoryName;
      }
      set => UAModelDesignerProject.BuildOutputDirectoryName = value;
    }

    internal string BuildOutputDirectoryPath => ReplaceTokenAndReturnFullPath(BuildOutputDirectoryName, UAModelDesignerProject.Name, m_SolutionHomeDirectory.BaseDirectory);

    internal ModelDesign Model { get; private set; }

    /// <summary>
    /// Saves the project to the specified directory.
    /// </summary>
    /// <param name="solutionDirectory">The solution directory.</param>
    /// <returns></returns>
    internal bool Save()
    {
      return Model.SaveModel(CalculateEffectiveAbsoluteModelFilePath());
    }

    /// <summary>
    /// Builds the project and write any massages to specified output.
    /// </summary>
    /// <param name="output">The output containing text sent by the compiler.</param>
    internal void Build(TextWriter output)
    {
      try
      {
        lock (m_BuildLockObject)
        {
          output.WriteLine(string.Format(Resources.Build_project_name, this.Text));
          output.WriteLine(string.Format(Resources.Build_started_at, System.DateTime.Now.ToString()));
          // some verification at the beginning
          DirectoryInfo dirinfo = new DirectoryInfo(BuildOutputDirectoryPath);
          if (!dirinfo.Exists)
            Directory.CreateDirectory(BuildOutputDirectoryPath);
          string _filePath = CalculateEffectiveAbsoluteModelFilePath();
          if (!new FileInfo(_filePath).Exists)
          {
            string msg = string.Format(Resources.BuildError_Fie_DoesNotExist, _filePath);
            output.WriteLine(msg);
            this.MessageBoxHandling.Show(msg, Resources.Build_Caption, MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
          }
          if (!new FileInfo(CSVFilePath).Exists)
          {
            string msg = string.Format(Resources.BuildError_Fie_DoesNotExist_doyouwanttocreateone, CSVFilePath);
            if (this.MessageBoxHandling.Show(msg, "Build", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
              //we are creating an blank file (one empty line inside)
              StreamWriter myCsvFile = new StreamWriter(CSVFilePath, false);
              using (myCsvFile)
              {
                myCsvFile.WriteLine(" ");
                myCsvFile.Flush();
                myCsvFile.Close();
              }
            }
            else
            {
              output.WriteLine(string.Format(Resources.BuildError_Fie_DoesNotExist, CSVFilePath));
              return;
            }
          }
          string argument = string.Format(Properties.Settings.Default.Build_ProjectCompilationString, _filePath, CSVFilePath, BuildOutputDirectoryPath);
          string CompilationExecutable = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), Properties.Settings.Default.ProjectCompilationExecutable);
          ProcessStartInfo myStartInfo = new System.Diagnostics.ProcessStartInfo(CompilationExecutable)
          {
            Arguments = argument,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
          };
          output.WriteLine();
          output.Write(CompilationExecutable);
          output.Write(" ");
          output.WriteLine(argument);
          output.WriteLine();
          Process myBuildProcess = new Process
          {
            StartInfo = myStartInfo
          };
          if (!myBuildProcess.Start())
            this.MessageBoxHandling.Show(Resources.Build_click_ok_when_build_has_finished);
          else
          {
            myBuildProcess.WaitForExit();
            string outputfrombuildprocess = myBuildProcess.StandardOutput.ReadToEnd();
            string erroroutputfrombuildprocess = myBuildProcess.StandardError.ReadToEnd();
            if (!string.IsNullOrEmpty(erroroutputfrombuildprocess))
            {
              erroroutputfrombuildprocess = string.Format(Resources.BuildError_error_occured, erroroutputfrombuildprocess);
            }
            else
            {
              erroroutputfrombuildprocess = Resources.Build_project_ok;
            }
            outputfrombuildprocess += erroroutputfrombuildprocess;
            if (!string.IsNullOrEmpty(outputfrombuildprocess))
              output.WriteLine(outputfrombuildprocess);
          }
        }
        output.WriteLine(string.Format(Resources.Build_ended_at, System.DateTime.Now.ToString()));
        output.WriteLine();
        // it is also possible in the future to use Opc.Ua.ModelCompiler.ModelGenerator2 gen = new Opc.Ua.ModelCompiler.ModelGenerator2();
        // or it can be done as: C:\vs\UAtrunk\Source\Utilities\ModelDesigner\Program.cs, function ProcessCommandLine2
      }
      catch (Exception ex)
      {
        output.WriteLine(Resources.BuildError_nocontinuation + "\n\r" + ex.Message, "Build", MessageBoxButtons.OK, MessageBoxIcon.Error);
      }
    }

    internal void AddNode2AddressSpace(IAddressSpaceCreator space)
    {
      foreach (ModelDesign item in this)
        item.AddNode2AddressSpace(space);
    }

    internal ITypeDesign Find(XmlQualifiedName myType)
    {
      foreach (ModelDesign node in this)
      {
        ITypeDesign ret = node.FindType(myType);
        if (ret != null)
          return ret;
      }
      return null;
    }

    #endregion public

    #region IProjectModel

    public void Remove()
    {
      Parent.Remove(this);
    }

    #endregion IProjectModel
  }
}