import { type ReactNode } from 'react';
import './WarningMessage.css';

interface WarningMessageProps {
  type?: 'warning' | 'error' | 'info' | 'success';
  title?: string;
  children: ReactNode;
  id?: string;
}

/**
 * Fluid text-wrapping alert component.
 * Eliminates horizontal data clipping on any viewport width.
 */
export function WarningMessage({ type = 'warning', title, children, id }: WarningMessageProps) {
  const icons: Record<string, string> = {
    warning: '⚠️',
    error: '🚫',
    info: 'ℹ️',
    success: '✅',
  };

  return (
    <div id={id} className={`warning-message warning-message--${type}`} role="alert">
      <span className="warning-message__icon" aria-hidden="true">
        {icons[type]}
      </span>
      <div className="warning-message__content">
        {title && <strong className="warning-message__title">{title}</strong>}
        <div className="warning-message__body">{children}</div>
      </div>
    </div>
  );
}
