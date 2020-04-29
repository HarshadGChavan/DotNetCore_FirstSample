function confirmDelete(uniqueID,isDeleteClicked)
{
 var deletespan = "deleteSpan_"+uniqueID;
 var confirmdeletespan = "confirmDeleteion_"+uniqueID;

 if(isDeleteClicked) {
     $('#'+deletespan).hide();
     $('#'+confirmdeletespan).show();
 } else{
    $('#'+deletespan).show();
    $('#'+confirmdeletespan).hide();
 }
}