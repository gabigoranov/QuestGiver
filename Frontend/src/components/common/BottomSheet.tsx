import { useEffect } from "react";

type BottomSheetProps = {
  isOpen: boolean;
  onClose: () => void;
  children: React.ReactNode;
};


/**
 * Used in certain areas of the app to display quick info
 *
 * @export
 * @param {BottomSheetProps} {
 *   isOpen,
 *   onClose,
 *   children,
 * }
 * @return {*} 
 */
export default function BottomSheet({
  isOpen,
  onClose,
  children,
}: BottomSheetProps) {
  // Prevent background scroll
  useEffect(() => {
    if (isOpen) {
      document.body.style.overflow = "hidden";
    } else {
      document.body.style.overflow = "";
    }
  }, [isOpen]);

  return (
    <>
      {/* Backdrop */}
      <div
        className={`fixed inset-0 bg-black/40 transition-opacity z-40 ${
          isOpen ? "opacity-100" : "opacity-0 pointer-events-none"
        }`}
        onClick={onClose}
      />

      {/* Sheet */}
      <div
        className={`fixed bottom-0 left-0 right-0 z-50 transform transition-transform duration-300 ${
          isOpen ? "translate-y-0" : "translate-y-full"
        }`}
      >
        <div className="bg-background rounded-t-2xl p-4 shadow-lg">
          {/* Drag handle */}
          <div className="w-12 h-1.5 bg-muted-foreground/40 mx-auto mb-4 rounded-full" />

          {children}
        </div>
      </div>
    </>
  );
}