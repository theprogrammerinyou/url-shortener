import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import {
  Box, Typography, Grid, Button, Card, CardContent, CardActions,
  List, ListItem, ListItemIcon, ListItemText, Dialog, DialogTitle, DialogContent, DialogActions
} from '@mui/material';
import CheckIcon from '@mui/icons-material/Check';
import StarIcon from '@mui/icons-material/Star';
import ArrowBackIcon from '@mui/icons-material/ArrowBack';
import { useApp } from '../context/AppContext';

export default function PricingPage() {
  const navigate = useNavigate();
  const { showSnackbar } = useApp();
  const [successOpen, setSuccessOpen] = useState(false);

  const handleStartTrial = () => {
    setSuccessOpen(true);
    showSnackbar('14-day Pro trial successfully activated!');
  };

  const plans = [
    {
      title: 'Free',
      price: '$0',
      period: 'forever',
      description: 'Basic shortening for individuals.',
      features: [
        'Up to 50 links / month',
        'Basic link analytics',
        'Standard redirection speed',
        '30-day link expiry',
      ],
      buttonText: 'Current Plan',
      buttonVariant: 'outlined' as const,
      disabled: true,
    },
    {
      title: 'Pro Trial',
      price: '$0',
      period: 'for 14 days, then $19/mo',
      description: 'Advanced features for professionals.',
      features: [
        'Unlimited custom aliases',
        'Advanced link analytics & charts',
        'QR Code generation & tracking',
        'Custom domain support',
        'Priority redirection support',
        'No advertisements',
      ],
      buttonText: 'Start 14-Day Free Trial',
      buttonVariant: 'contained' as const,
      highlighted: true,
      action: handleStartTrial,
    },
    {
      title: 'Enterprise',
      price: 'Custom',
      period: 'tailored pricing',
      description: 'Scale securely with advanced controls.',
      features: [
        'Dedicated SLA & custom domains',
        '99.99% uptime guarantee',
        'Team collaboration & shared keys',
        'Dedicated account manager',
        'Custom API integration limits',
      ],
      buttonText: 'Contact Sales',
      buttonVariant: 'outlined' as const,
      action: () => showSnackbar('Enterprise sales query submitted!'),
    },
  ];

  return (
    <Box sx={{ maxWidth: 1100, mx: 'auto', py: 2 }}>
      <Box sx={{ mb: 4, display: 'flex', alignItems: 'center', gap: 2 }}>
        <Button startIcon={<ArrowBackIcon />} onClick={() => navigate(-1)}>
          Back
        </Button>
      </Box>

      <Box sx={{ textAlign: 'center', mb: 6 }}>
        <Typography variant="h4" fontWeight={800} sx={{ mb: 1.5 }}>
          Simple, Transparent Pricing
        </Typography>
        <Typography variant="body1" color="text.secondary" sx={{ maxWidth: 600, mx: 'auto' }}>
          Choose the plan that fits your link management needs. Start your free trial today.
        </Typography>
      </Box>

      <Grid container spacing={4} alignItems="stretch">
        {plans.map((plan) => (
          <Grid item xs={12} md={4} key={plan.title}>
            <Card
              sx={{
                height: '100%',
                display: 'flex',
                flexDirection: 'column',
                borderRadius: 4,
                border: '1px solid',
                borderColor: plan.highlighted ? 'primary.main' : 'divider',
                boxShadow: plan.highlighted ? '0 10px 30px -10px rgba(79,70,229,0.2)' : 'none',
                position: 'relative',
                transform: plan.highlighted ? { md: 'scale(1.03)' } : 'none',
                transition: 'transform 0.2s',
              }}
            >
              {plan.highlighted && (
                <Box
                  sx={{
                    position: 'absolute',
                    top: 16,
                    right: 16,
                    bgcolor: 'primary.main',
                    color: '#fff',
                    px: 1.5,
                    py: 0.5,
                    borderRadius: 2,
                    display: 'flex',
                    alignItems: 'center',
                    gap: 0.5,
                  }}
                >
                  <StarIcon sx={{ fontSize: 16 }} />
                  <Typography variant="caption" fontWeight={700}>Popular</Typography>
                </Box>
              )}

              <CardContent sx={{ p: 4, flexGrow: 1 }}>
                <Typography variant="h5" fontWeight={800} sx={{ mb: 1 }}>
                  {plan.title}
                </Typography>
                <Typography variant="body2" color="text.secondary" sx={{ mb: 3, minHeight: 40 }}>
                  {plan.description}
                </Typography>
                <Box sx={{ display: 'flex', alignItems: 'baseline', mb: 1 }}>
                  <Typography variant="h3" fontWeight={800} color="text.primary">
                    {plan.price}
                  </Typography>
                  <Typography variant="body2" color="text.secondary" sx={{ ml: 1 }}>
                    / {plan.period}
                  </Typography>
                </Box>

                <List sx={{ mt: 3, display: 'flex', flexDirection: 'column', gap: 1 }}>
                  {plan.features.map((feature) => (
                    <ListItem key={feature} disablePadding sx={{ itemsAlign: 'flex-start' }}>
                      <ListItemIcon sx={{ minWidth: 32, mt: 0.25 }}>
                        <CheckIcon color={plan.highlighted ? 'primary' : 'action'} sx={{ fontSize: 20 }} />
                      </ListItemIcon>
                      <ListItemText
                        primary={feature}
                        primaryTypographyProps={{ variant: 'body2', color: 'text.secondary' }}
                      />
                    </ListItem>
                  ))}
                </List>
              </CardContent>

              <CardActions sx={{ p: 4, pt: 0 }}>
                <Button
                  fullWidth
                  variant={plan.buttonVariant}
                  onClick={plan.action}
                  disabled={plan.disabled}
                  size="large"
                  sx={{ py: 1.5, borderRadius: 3, fontWeight: 700 }}
                >
                  {plan.buttonText}
                </Button>
              </CardActions>
            </Card>
          </Grid>
        ))}
      </Grid>

      <Dialog open={successOpen} onClose={() => setSuccessOpen(false)} maxWidth="xs" fullWidth>
        <DialogTitle sx={{ fontWeight: 700, textAlign: 'center' }}>Trial Activated!</DialogTitle>
        <DialogContent sx={{ display: 'flex', flexDirection: 'column', alignItems: 'center', gap: 2, py: 3 }}>
          <Typography variant="body2" color="text.secondary" align="center">
            Your 14-day free trial of <strong>Swft Pro</strong> is now active. Enjoy unlimited custom aliases, QR codes, and advanced analytics!
          </Typography>
        </DialogContent>
        <DialogActions sx={{ justifyContent: 'center', px: 3, pb: 3 }}>
          <Button variant="contained" onClick={() => { setSuccessOpen(false); navigate('/'); }}>
            Start Shortening
          </Button>
        </DialogActions>
      </Dialog>
    </Box>
  );
}
