

public interface NPipeIComposite : NPipeIImportable
{
     NPipeIImportable Input 
     {
         get; set;
     }
     NPipeIImportable[] GetAllInputs();
}