import type { Config } from 'tailwindcss'

export default {
  content: ['./index.html', './src/**/*.{ts,tsx}'],
  theme: {
    extend: {
      colors: {
        'bg-base': '#faf9f5',
        'bg-dark': '#0F0F10',
        'bg-elevated': '#FFFFFF',
        primary: {
          DEFAULT: '#B8341B',
          hover: '#9A2A14',
        },
        accent: '#C9A961',
        'text-primary': '#0F0F10',
        'text-on-dark': '#F5F5F0',
        'text-muted': '#6B6963',
        border: '#E8E5DC',
        success: '#5B7A4B',
        error: '#8B2615',
        warning: '#C9A961',
      },
      fontFamily: {
        display: ['Oswald', 'sans-serif'],
        sans: ['Inter', '-apple-system', 'BlinkMacSystemFont', 'sans-serif'],
      },
      borderRadius: {
        md: '6px',
        lg: '8px',
      },
    },
  },
  plugins: [],
} satisfies Config
