RMA DIALOG SYSTEM DOCUMENTATION
Project: RMA Password Manager
Last Updated: April 2026

OVERVIEW
The RMA Dialog System is a custom UI component designed to replace the standard Windows MessageBox. It aligns with the RMA design language featuring rounded corners (24px), high-contrast typography, and specialized state colors for security contexts.

USAGE API (C#)
The system is accessed via static methods. You do not need to instantiate the RmaDialog class manually.

A. Information (RmaDialog.Info)
Used for success states or general updates.
Usage: RmaDialog.Info("Success", "Vault successfully initialized.");
Theme: Green (#3A5A40) | Icon: Info24

B. Warning (RmaDialog.Warn)
Used for actions requiring user confirmation. Returns a boolean.
Usage: if (RmaDialog.Warn("Confirm", "Are you sure?")) { ... }
Theme: Dusty Rose (#B5838D) | Icon: Warning24

C. Error (RmaDialog.Error)
Used for critical failures or validation errors.
Usage: RmaDialog.Error("Access Denied", "Incorrect Master PIN.");
Theme: Red (#D00000) | Icon: ErrorCircle24

TECHNICAL NOTES
The dialog uses 'ShowDialog()' which is modal. This means the user cannot click the main window behind it until the dialog is closed. This is intended for security prompts and critical confirmations.