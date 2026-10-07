import {useEffect, type ReactNode, useRef} from "react";
import {createPortal} from "react-dom";

type Props = {
  children: ReactNode
  onConfirm: () => void,
  onClose: () => void,
}

export const Modal = ({children, onConfirm, onClose}: Props) => {
  const confirmRef = useRef<HTMLButtonElement>(null);

  useEffect(() => {
    confirmRef.current?.focus();          // runs once, after the modal is in the page
  }, []);

  useEffect(() => {
    const onKey = (e: KeyboardEvent) => { if (e.key === 'Escape') onClose() }
    window.addEventListener('keydown', onKey)
    return () => window.removeEventListener('keydown', onKey)
  }, [onClose])

  return createPortal(
    <div className="backdrop" onClick={onClose}>
      <div className="dialog" role="dialog" onClick={e => e.stopPropagation()}>
        <h2>Are you sure?</h2>
        {children}
        <button onClick={onClose}>Cancel</button>
        <button
          onClick={onConfirm}
          ref={confirmRef}
          >Confirm</button>
      </div>
    </div>,
    document.body
  )
}
