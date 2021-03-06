﻿//___________________________________________________________________________________
//
//  Copyright (C) 2019, Mariusz Postol LODZ POLAND.
//
//___________________________________________________________________________________

using CAS.UA.Model.Designer.Properties;
using System.Collections.Generic;

namespace CAS.UA.Model.Designer.Wrappers
{
  internal partial class MethodDesign: InstanceDesign<Wrappers4ProperyGrid.MethodDesign, Opc.Ua.ModelCompiler.MethodDesign>
  {
    #region creators
    /// <summary>
    /// Initializes a new instance of the <see cref="MethodDesign"/> class.
    /// </summary>
    public MethodDesign()
      : base( new CAS.UA.Model.Designer.Wrappers4ProperyGrid.MethodDesign( new Opc.Ua.ModelCompiler.MethodDesign() ) )
    {
      CreateFolders( null, null );
    }
    /// <summary>
    /// Initializes a new instance of the <see cref="MethodDesign"/> class.
    /// </summary>
    /// <param name="node">The modeled node.</param>
    public MethodDesign( Opc.Ua.ModelCompiler.MethodDesign node )
      : base( new Wrappers4ProperyGrid.MethodDesign( node ), node )
    {
      CreateFolders( node.InputArguments, node.OutputArguments );
    }
    #endregion

    #region public
    public ParametersFolder InputArguments { get { return m_inputArgumentsFields; } }
    public ParametersFolder OutputArguments { get { return m_outputArgumentsFields; } }
    public override object ModelDesignerNode
    {
      get
      {
        Opc.Ua.ModelCompiler.MethodDesign node = (Opc.Ua.ModelCompiler.MethodDesign)base.ModelDesignerNode;
        node.InputArguments = m_inputArgumentsFields.CreateParameters();
        node.OutputArguments = m_outputArgumentsFields.CreateParameters();
        return node;
      }
    }
    public override Dictionary<FolderType, IEnumerable<IModelNodeAdvance>> GetFolders()
    {
      Dictionary<FolderType, IEnumerable<IModelNodeAdvance>> tobereturned = base.GetFolders();
      tobereturned.Add( FolderType.InputArguments, InputArguments );
      tobereturned.Add( FolderType.OutputArguments, OutputArguments );
      return tobereturned;
    }
    /// <summary>
    /// Gets the name of the help topic.
    /// </summary>
    /// <value>The name of the help topic.</value>
    public override string HelpTopicName
    {
      get { return Resources.NodeClasses_Objects_Method; }
    }
    /// <summary>
    /// Gets the node class.
    /// </summary>
    /// <value>The node class.</value>
    public override NodeClassesEnum NodeClass
    {
      get { return NodeClassesEnum.Method; }
    }
    #endregion

    #region Private and Protected
    private void CreateFolders( Opc.Ua.ModelCompiler.Parameter[] input, Opc.Ua.ModelCompiler.Parameter[] output )
    {
      m_inputArgumentsFields = new ParametersFolder
        (
          input, WrapperResources.ParametersFolderInputTreeNodeText,
          WrapperResources.ParametersFolderInputTreeNodeToolTipText
        );
      Add( m_inputArgumentsFields );
      m_outputArgumentsFields = new ParametersFolder
        (
          output,
          WrapperResources.ParametersFolderOutputTreeNodeText,
          WrapperResources.ParametersFolderOutputTreeNodeToolTipText
        );
      Add( m_outputArgumentsFields );
    }
    private ParametersFolder m_inputArgumentsFields = null;
    private ParametersFolder m_outputArgumentsFields = null;
    protected override void AddNode2AddressSpace( IAddressSpaceCreator space )
    {
      base.AddNode2AddressSpace( space );
      foreach ( var item in m_inputArgumentsFields )
      {
        //TODO add argument to AddressSpace
      }
      foreach ( var item in m_outputArgumentsFields )
      {
        //TODO add argument to AddressSpace
      }
    }
    #endregion Private and Protected

  }
}
