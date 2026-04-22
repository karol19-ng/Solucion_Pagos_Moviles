/* eslint-disable */
import * as Router from 'expo-router';

export * from 'expo-router';

declare module 'expo-router' {
  export namespace ExpoRouter {
    export interface __routes<T extends string = string> extends Record<string, unknown> {
      StaticRoutes: `/` | `/_sitemap` | `/admin` | `/admin/audit-logs` | `/admin/commissions` | `/admin/metrics` | `/admin/notifications` | `/admin/user-form` | `/admin/users` | `/auth` | `/auth/forgot-password` | `/auth/login` | `/auth/register` | `/constants/api` | `/core` | `/core/approval-detail` | `/core/approvals` | `/core/limits` | `/core/qr-scanner` | `/core/reports` | `/hooks/useAuth` | `/hooks/useRole` | `/portal` | `/portal/balance` | `/portal/history` | `/portal/home` | `/portal/pay-services` | `/portal/profile` | `/portal/transfer` | `/portal/unsubscribe` | `/services/authService` | `/styles/theme` | `/utils/storage` | `/utils/validations`;
      DynamicRoutes: `/portal/details/${Router.SingleRoutePart<T>}`;
      DynamicRouteTemplate: `/portal/details/[id]`;
    }
  }
}
